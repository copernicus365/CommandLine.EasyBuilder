using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq.Expressions;
using System.Reflection;

namespace CommandLine.EasyBuilder.Internal;

/// <summary>
/// A big heavy hitter, handles much of the reflection magic needed to make this work.
/// All members here are reflection related.
/// </summary>
public class CmdModelReflectionHelper
{
	public static CommandAttribute GetCommandAttribute(Type modelType)
		=> modelType.GetCustomAttributes(typeof(CommandAttribute), true).FirstOrDefault() as CommandAttribute;

	/// <summary>
	/// On input type, looks for and returns a method named 'Handle' or 'HandleAsync',
	/// or null if none.
	/// </summary>
	/// <param name="modelType">Model type</param>
	/// <returns></returns>
	/// <exception cref="ArgumentException">Return type MUST be void or Task</exception>
	public static (MethodInfo method, bool handleIsAsync) GetHandleMethod(Type modelType)
	{
		MethodInfo method = modelType.GetMethod("Handle");
		if(method == null) {
			method = modelType.GetMethod("HandleAsync");
			if(method == null)
				return default;
		}

		// note: even if named 'HandleAsync', if return type if void, we still let it work (call it as non-async / like 'Handle') 
		bool isVoidRetType = method.ReturnType == typeof(void);
		bool isTaskRetType = !isVoidRetType && method.ReturnType == typeof(Task);

		if(!isVoidRetType && !isTaskRetType)
			throw new ArgumentException("Handle method must return void or Task");

		return (method, !isVoidRetType);
	}

	public static CmdModelInfo GetCmdModelInfo(Type modelCmdType, bool throwIfNull = true) //where T : class, new()
	{
		PropertyInfo[] props = modelCmdType.GetProperties() ?? [];

		if(props == null) {
			if(!throwIfNull)
				return null;
			throw new ArgumentException("Type if invalid, no attributes / properties found");
		}

		CmdProp[] ogroup = [.. props.Select(GetCmdProperties).Where(v => v != null)];

		MethodInfo parseResultSetter = GetParseResultPropertySetter(props);

		CommandAttribute cmdAttr = GetCommandAttribute(modelCmdType);

		CmdModelInfo info = new() {
			ModelType = modelCmdType,
			CommandAttr = cmdAttr,
			Props = ogroup,
			ParseResultSetter = parseResultSetter
		};

		(MethodInfo method, bool handleIsAsync) = GetHandleMethod(modelCmdType);
		if(method != null)
			info.SetHandle(method, handleIsAsync);

		Command cmd = info.GetCommand();

		return info;
	}



	static MethodInfo GetParseResultPropertySetter(PropertyInfo[] props)
	{
		MethodInfo parseResultSetter = props.FirstOrDefault(p =>
			p.PropertyType == typeof(ParseResult) &&
			p.CanWrite &&
			(p.Name == "ParseResult" || p.Name == "ParsedResult") &&
			p.SetMethod != null)?.SetMethod;
		// lets not demand Public. Many cases may want to keep private
		// && p.SetMethod.IsPublic)
		return parseResultSetter;
	}

	public static object TryGetOptionFromStaticMethod(PropertyInfo pi, bool isOption, Type assertExpectedType)
	{
		Type declaringType = pi.DeclaringType;
		if(declaringType == null)
			return null;

		string appdx1 = isOption ? "Option" : "Argument";
		string appdx2 = isOption ? "Opt" : "Arg";

		string propName = pi.Name;
		string[] methodNames = [$"Get{propName}{appdx1}", $"Get{propName}{appdx2}", $"Get{propName}"];

		Type propertyType = pi.PropertyType;
		Type genTyp = isOption ? typeof(Option<>) : typeof(Argument<>);
		Type optionGenericType = genTyp.MakeGenericType(propertyType);

		if(assertExpectedType != null && optionGenericType != assertExpectedType)
			throw new ArgumentException($"Get static option or argument type is invalid for property '{propName}' ({propertyType.FullName})");

		// Get static public methods
		MethodInfo[] staticMethods = declaringType.GetMethods(BindingFlags.Static | BindingFlags.Public);
		MethodInfo targetMethod = null;

		for(int i = 0; i < methodNames.Length; i++)
			if(setMI(methodNames[i]))
				break;

		if(targetMethod == null)
			return null;

		// Invoke static method (no instance, no args)
		object result = targetMethod.Invoke(null, null);
		return result; //return (Option?)result;

		// Look for matching method: name matches, return type is Option<T> or Option<T>, no parameters
		bool setMI(string methodNm)
		{
			targetMethod = staticMethods
				 .Where(m => m.ReturnType == optionGenericType)
				 .Where(m => m.GetParameters().Length == 0)
				 .FirstOrDefault(m => m.Name == methodNm);
			return targetMethod != null;
		}
	}

	static CmdProp GetCmdProperties(PropertyInfo pi)
	{
		CLPropertyAttribute c = pi.GetCustomAttribute(typeof(CLPropertyAttribute), true) as CLPropertyAttribute;

		OptionAttribute optattr = c as OptionAttribute;
		bool isOpt = optattr != null;
		if(!isOpt) {
			if(c is not ArgumentAttribute argAttr)
				return null;
		}

		Type propTyp = pi.PropertyType;

		Type propTypeFromNullable = !propTyp.IsValueType ? null : Nullable.GetUnderlyingType(propTyp);

		bool isNullable = propTypeFromNullable != null;

		Type constructedGenType = (isOpt ? typeof(Option<>) : typeof(Argument<>)).MakeGenericType(propTyp);
		Option opt = null;
		Argument arg = null;

		bool optionComesFromDirectStaticMethod = string.IsNullOrEmpty(c.Name);
		if(optionComesFromDirectStaticMethod) {
			object objFromSt = TryGetOptionFromStaticMethod(pi, isOpt, constructedGenType);
			if(objFromSt == null)
				throw new InvalidOperationException($"Property '{pi.Name}' is marked to get {(isOpt ? "Option" : "Argument")} from static method, but no valid static method found.");

			if(isOpt) {
				opt = objFromSt as Option;
				c.Name = opt.Name;
			}
			else {
				arg = objFromSt as Argument;
				c.Name = arg.Name;
			}

			CmdProp prop1 = new(propTyp, isNullable, pi, c, opt, arg, defaultOfTVal: null);
			return prop1;
		}

		if(isOpt) {
			opt = Activator.CreateInstance(constructedGenType, c.Name) as Option;
			opt.Description = c.Description;
		}
		else {
			arg = Activator.CreateInstance(constructedGenType, c.Name) as Argument;
			arg.Description = c.Description;
		}

		if(c.MinArity > 0 || c.MaxArity > 0) {
			int min = c.MinArity;
			int max = c.MaxArity;
			if(min < 0 || max < min)
				throw new ArgumentOutOfRangeException("MinArgs or MaxArgs values out of range");

			ArgumentArity arity = new(min, max);
			if(isOpt)
				opt.Arity = arity;
			else
				arg.Arity = arity;
		}

		if(isOpt) {
			if(c.Required == true)
				opt.Required = true;

			if(!string.IsNullOrEmpty(c.Alias))
				opt.Aliases.Add(c.Alias);

			if(c.AllowMultipleArgumentsPerToken)
				opt.AllowMultipleArgumentsPerToken = true;
		}

		if(c.HelpName != null) {
			if(isOpt)
				opt.HelpName = c.HelpName;
			else
				arg.HelpName = c.HelpName;
		}

		// NEED to have default(T)
		// SET THESE FOR NULL, but then FIX for value types next
		object defaultT = null;
		bool hasDefPropSet = c.DefVal != null;

		if(hasDefPropSet && // if is NULL, then no use...
			propTyp.IsValueType) {

			// hasDefPropSet incorrectly said TRUE bec only tested NULL, fix for val type
			// must create an instance to compare

			defaultT = Activator.CreateInstance(propTyp);

			if(defaultT != null
				&& c.DefVal.Equals(defaultT)) {
				hasDefPropSet = false; // have OFFICIALLY stopped val types from saying they had a value set
			}
		} // no else needed, initial values handled not value type

		if(hasDefPropSet) {
			Delegate dlg = GetArgResFuncReturnsObjectVal(propTyp, c.DefVal);
			PropertyInfo getValProp = constructedGenType.GetProperty("DefaultValueFactory");
			getValProp.SetValue(isOpt ? opt : arg, dlg);
		}

		CmdProp prop = new(propTyp, isNullable, pi, c, opt, arg, defaultT);
		return prop;
	}

	static Delegate GetArgResFuncReturnsObjectVal(Type genericPropTyp, object val)
	{
		// thank you, GROK
		// Build Func<ArgumentResult, T> (ArgumentResult arg) => (T)val (as a constant expression, ignoring the input)
		Type funcType = typeof(Func<,>).MakeGenericType(typeof(ArgumentResult), genericPropTyp);
		ParameterExpression param = Expression.Parameter(typeof(ArgumentResult)); //, "arg");
		Expression body = Expression.Constant(val, genericPropTyp);
		LambdaExpression lambdaExpr = Expression.Lambda(funcType, body, param);
		Delegate constantFunc = lambdaExpr.Compile();
		return constantFunc;
	}

	/// <summary>
	/// Critical method. Used internally by AutoAttributesBinder to perform the
	/// ultimate binding and reading of types / input into custom class.
	/// 
	/// </summary>
	/// <param name="parseRes"></param>
	/// <param name="p"></param>
	/// <param name="item"></param>
	/// <remarks>
	/// FUTURE WORK! --> We need to DETECT scenarios where a property can have a default
	/// set directly on the model class property, and was not input. This is different from
	/// default value, as that has the major benefit of telling the user. HOWEVER, it would be
	/// great to allow our model / class based system **TO NOT OVERWRITE** a property value
	/// set directly on the class (eg `public int Age { get; set; } = 22;`). CURRENTLY we walk
	/// through each attribute property and set the values. Would be great to allow for above scenario
	/// without overwriting above. ... hmmm ... could pry do this AT INIT time, create an instance of
	/// model, and detect any that have a set value, and save that on CmdProp with `HasPreSetValue` or something.
	/// THEN: if input value is default(TProp), 1 last check: to validate it wasn't actually input as default...
	/// (that's difficult tho)
	/// </remarks>
	public static void SetVal(ParseResult parseRes, CmdProp p, object item)
	{
		string name = p.attr.Name;
		Type propTyp = p.type;

		// >> get value via attr-name
		// >> get value via Option<T> / Argument<T> ... I quit because getting typeof arg added pain
		MethodInfo method = (typeof(ParseResult)).GetMethod("GetValue", [typeof(string)]).MakeGenericMethod(propTyp);
		object prm = name;
		object value = method.Invoke(parseRes, [prm]);

		if(value != null && propTyp.IsValueType && p.isNullable) {
		}

		bool hasDefVal = p.attr.DefVal != null;
		if(!hasDefVal) {
		}
		else if(value == null || value.Equals(p.defaultOfTVal)) {
			// --- IF NULL or DEFAULT ---
			var aliases = p.option.Aliases;

			if(!p.IsNonNullableValueTypeAndValEqualsDefaultTButNotDefValue(value)) { // || (aliases?.Count ?? 0) < 1) {
				value = p.attr.DefVal;
			}
			else {
				// NOTE #1
				int tknMatchCount = parseRes.Tokens.Count(t =>
					t.Type == TokenType.Option &&
					aliases.Any(a => string.Equals(a, t.Value, StringComparison.OrdinalIgnoreCase)));
				if(tknMatchCount < 1) // if(tkn == null) see bug below can't test!
					value = p.attr.DefVal;  // token was NOT in input, so DO use DefVal
			}
		}

		p.pi.SetValue(item, value);
	}
}

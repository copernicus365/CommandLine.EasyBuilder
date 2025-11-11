using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq.Expressions;
using System.Reflection;

using CommandLine.EasyBuilder.Private;

namespace CommandLine.EasyBuilder.Internal;

public class CmdModelReflectionHelper
{
	public static CommandAttribute GetCommandAttribute<T>()
		=> typeof(T).GetCustomAttributes(typeof(CommandAttribute), true).FirstOrDefault() as CommandAttribute;

	public static object SetHandleMethod(CmdModelInfo info)
	{
		Type type = info.Type; // typeof(TAuto);

		MethodInfo method = type.GetMethod("Handle");
		if(method == null) {
			method = type.GetMethod("HandleAsync");
			if(method == null)
				return null;
		}

		bool isVoidRetType = method.ReturnType == typeof(void);
		bool isTaskRetType = !isVoidRetType && method.ReturnType == typeof(Task);

		if(!isVoidRetType && !isTaskRetType)
			throw new ArgumentException("Handle method must return void or Task");

		info.SetHandle(method, !isVoidRetType);

		var firstP = method.GetParameters().FirstOrDefault();
		var firstPT = firstP?.ParameterType;

		object _handle = null;
		return _handle;

		//if(isVoidRetType) {
		//	// auto gen IGNore
		//	//Action<ParseResult> act1 = (pr) => {
		//	//	object inst = Activator.CreateInstance(type);
		//	//	method.Invoke(inst, [pr]);
		//	//};

		//	Action<TAuto, ParseResult> handle = (TAuto v, ParseResult pr) => {
		//		info.Method.Invoke(v, [pr]);
		//	};

		//	Action<ParseResult> act1 = (pr) => {
		//		handle
		//		////TAuto inst = Activator.CreateInstance<TAuto>();
		//		//method.Invoke(inst, [pr]);
		//	};

		//	_handle = handle;
		//	//cmd.SetAction(
		//}
		//else {
		//	throw new NotImplementedException();
		//	//Func<TAuto, Task> handle = async (TAuto v) => {
		//	//	object res = info.Method.Invoke(v, null);
		//	//	Task result = res as Task;
		//	//	await result;
		//	//};
		//	//_handle = handle;
		//}

		//info.SetHandle(_handle);
	}

	public static CmdModelInfo<T> GetCmdModelInfo<T>(bool throwIfNull = true) where T : class, new()
	{
		Type type = typeof(T);

		PropertyInfo[] props = type.GetProperties() ?? [];

		if(props == null) {
			if(!throwIfNull)
				return null;
			throw new ArgumentException("Type if invalid, no attributes / properties found");
		}

		CmdProp[] ogroup = [.. props.Select(GetCmdProperties).Where(v => v != null)];

		MethodInfo parseResultSetter = props.FirstOrDefault(p =>
			p.PropertyType == typeof(ParseResult) &&
			p.CanWrite &&
			(p.Name == "ParseResult" || p.Name == "ParsedResult") &&
			p.SetMethod != null &&
			p.SetMethod.IsPublic)?.SetMethod;

		CmdModelInfo<T> info = new() {
			Type = type,
			CommandAttr = GetCommandAttribute<T>(),
			Props = ogroup,
			ParseResultSetter = parseResultSetter
		};

		return info;
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
		//if(isNullable)
		//	propTyp = propTypeFromNullable; // <<< ERROR! messed up things, is fine ... I think ... w/out

		Type constructedGenType = (isOpt ? typeof(Option<>) : typeof(Argument<>)).MakeGenericType(propTyp);
		Option opt = null;
		Argument arg = null;

		bool optionComesFromDirectStaticMethod = c.Name.IsNulle();
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
			// ??? defaultOfTVal: null);
			// I believe CommandLineValueAttribute attr.defaultOfTVal is only used when c.DefVal is set
			// (below = hasDefPropSet). Since we don't of course, is OK *i think* to keep null
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

			if(c.Alias.NotNulle())
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

			//#warning ... FIX!
			//throw new NotImplementedException();

			//Option<int> zz;

			//zz.DefaultValueFactory

			//if(isOpt)
			//	opt.def.valu.SetDefaultValue(c.DefVal);
			//else
			//	arg.SetDefaultValue(c.DefVal);

			//if(isOpt)
			//	opt.SetDefaultValue(c.DefVal);
			//else
			//	arg.SetDefaultValue(c.DefVal);
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
			//value = Convert.ChangeType(value, propTyp); // ??
			// NOTE #0
		}

		bool hasDefVal = p.attr.DefVal != null; // CommandLineValueAttribute attr
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

				// NOTE #2
			}
		}

		p.pi.SetValue(item, value);
	}
}

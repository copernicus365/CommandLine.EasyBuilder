using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq.Expressions;
using System.Reflection;

using CommandLine.EasyBuilder.Private;

namespace CommandLine.EasyBuilder.Auto;

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
		
		CmdModelInfo<T> info = new() {
			Type = type,
			CommandAttr = GetCommandAttribute<T>(),
			Props = ogroup,
		};

		return info;
	}

	class Foo { public string Name { get; set; } }

	class Foo<T> : Foo { public Func<T> GetVal { get; set; } }


	static void SetStuff(Type genericPropTyp, string name)
	{

		Type constructedGenType = typeof(Foo<>).MakeGenericType(genericPropTyp);

		Foo opt = Activator.CreateInstance(constructedGenType) as Foo;

		opt.Name = name;
	}

	static CmdProp GetCmdProperties(PropertyInfo pi)
	{
		CommandLineValueAttribute c = pi.GetCustomAttribute(typeof(CommandLineValueAttribute), true) as CommandLineValueAttribute;

		OptionAttribute optattr = c as OptionAttribute;
		bool isOpt = optattr != null;
		if(!isOpt) {
			if(c is not ArgumentAttribute argAttr)
				return null;
		}

		Type propTyp = pi.PropertyType;

		Type propTypeFromNullable = !propTyp.IsValueType ? null : Nullable.GetUnderlyingType(propTyp);

		bool isNullable = propTypeFromNullable != null;
		if(isNullable)
			propTyp = propTypeFromNullable;

		Type constructedGenType = (isOpt ? typeof(Option<>) : typeof(Argument<>)).MakeGenericType(propTyp);
		Option opt = null;
		Argument arg = null;

		if(isOpt) {
			opt = Activator.CreateInstance(constructedGenType, c.Name) as Option;
			opt.Description = c.Description;
		}
		else {
			arg = Activator.CreateInstance(constructedGenType, c.Name) as Argument;
			arg.Description = c.Description;
		}

		if(isOpt) {
			if(c.Required == true)
				opt.Required = true;

			if(c.Alias.NotNulle())
				opt.Aliases.Add(c.Alias);
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

		CmdProp vv = new(propTyp, isNullable, pi, c, opt, arg, defaultT);
		return vv;

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
		//if(Verbose) { SetValVerbose(parseRes, p, item); return; }
		parseRes.va .GetValue(p.attr.Name)

#warning ... FIX!
		throw new NotImplementedException();

		object value = default;
		//p.IsOption
		//? parseRes.va .GetValueForOption(p.option)
		//: parseRes.GetValueForArgument(p.argument);

		Type propTyp = p.type;

		if(propTyp.IsValueType && p.isNullable)
			value = Convert.ChangeType(value, propTyp);

		bool hasDefVal = p.attr.DefVal != null;
		if(hasDefVal && (value == null || value.Equals(p.defaultOfTVal))) {

			var aliases = p.option.Aliases;

			if(!p.IsNonNullableValueTypeAndValEqualsDefaultTButNotDefValue(value) || (aliases?.Count ?? 0) < 1) {
				value = p.attr.DefVal;
			}
			else {
				// IN THIS CASE, *especially* for BOOLs(!), we HAVE to know if the option EXISTED in the input
				// otherwise there's NO WAY to know if DefVal should be used
				//Token tkn = parseRes.Tokens.FirstOrDefault(t =>
				int tknMatchCount = parseRes.Tokens.Count(t =>
					t.Type == TokenType.Option &&
					aliases.Any(a => string.Equals(a, t.Value, StringComparison.OrdinalIgnoreCase)));
				if(tknMatchCount < 1) // if(tkn == null) see bug below can't test!
					value = p.attr.DefVal;  // token was NOT in input, so DO use DefVal

				/*
				 * WHOA, bug in System.CommandLine! CanNOT test the most simplest of test, for obj == null! -> `tkn == null`
				 * Looks like they have an op_Equality BUG that THROWS test for null
tkn == null
'tkn == null' threw an exception of type 'System.NullReferenceException'
    Data: {System.Collections.ListDictionaryInternal}
    HResult: -2147467261
    HelpLink: null
    InnerException: null
    Message: "Object reference not set to an instance of an object."
    Source: "System.CommandLine"
    StackTrace: "   at System.CommandLine.Parsing.Token.op_Equality(Token left, Token right)"
    TargetSite: {Boolean op_Equality(System.CommandLine.Parsing.Token, System.CommandLine.Parsing.Token)}
				 */
			}
		}

		p.pi.SetValue(item, value);
	}
}

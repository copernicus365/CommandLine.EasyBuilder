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

	public static CmdModelInfo GetCmdModelInfo(Type modelCmdType, Command cmd = null, bool throwIfNoProperties = true)
	{
		PropertyInfo[] props = modelCmdType.GetProperties() ?? [];

		if(props == null) {
			if(!throwIfNoProperties)
				return null;
			throw new ArgumentException("Type is invalid, no attributes / properties found");
		}

		CmdProp[] ogroup = [.. props.Select(CmdPropGetter.PropToCmpProp).Where(v => v != null)];

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

		if(cmd == null)
			cmd = info.GetCommand();
		else
			info.SetCommand(cmd);

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

	public static Type GetNumericArrayElementType(Type type)
	{
		if(!type.IsArray || type.GetArrayRank() != 1)
			return null;

		Type typ = type.GetElementType();
		if(typ == null)
			return null;

		if(typ == typeof(int)
			|| typ == typeof(long)
			|| typ == typeof(double)
			|| typ == typeof(decimal))
			return typ;
		return null;
	}

	public static Delegate GetArgResFuncReturnsObjectVal(Type genericPropTyp, object val)
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
}

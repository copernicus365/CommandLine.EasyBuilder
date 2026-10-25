using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace CommandLine.EasyBuilder.Internal;

/// <summary>
/// A big heavy hitter, handles much of the reflection magic needed to make this work.
/// All members here are reflection related.
/// </summary>
public class CmdModelReflectionHelper
{
	/// <summary>
	/// What EasyBuilder reflects on in a command (view) model type: its constructors, its properties, and its
	/// methods (<c>Handle</c> / <c>HandleAsync</c>, and any static <c>Get{PropName}Option()</c>). Every model type
	/// parameter carries it, so a trimmed or native AOT build keeps those members.
	/// </summary>
	internal const DynamicallyAccessedMemberTypes ModelMembers =
		DynamicallyAccessedMemberTypes.PublicConstructors
		| DynamicallyAccessedMemberTypes.PublicProperties
		| DynamicallyAccessedMemberTypes.PublicMethods;

	public static CommandAttribute GetCommandAttribute(Type modelType)
		=> modelType.GetCustomAttributes(typeof(CommandAttribute), true).FirstOrDefault() as CommandAttribute;

	/// <summary>
	/// On input type, looks for and returns a method named 'Handle' or 'HandleAsync',
	/// or null if none.
	/// </summary>
	/// <param name="modelType">Model type</param>
	/// <returns></returns>
	/// <exception cref="ArgumentException">Return type MUST be void or Task</exception>
	public static (MethodInfo method, bool handleIsAsync) GetHandleMethod([DynamicallyAccessedMembers(ModelMembers)] Type modelType)
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

	public static CmdModelInfo GetCmdModelInfo([DynamicallyAccessedMembers(ModelMembers)] Type modelCmdType, Command cmd = null, bool throwIfNoProperties = true)
	{
		PropertyInfo[] props = modelCmdType.GetProperties() ?? [];

		if(props == null) {
			if(!throwIfNoProperties)
				return null;
			throw new ArgumentException("Type is invalid, no attributes / properties found");
		}

		bool hasCtorWithParams = ModelHasAnyConstructorsWithParameters(modelCmdType);

		MethodInfo parseResultSetter = GetParseResultPropertySetter(props);

		CommandAttribute cmdAttr = GetCommandAttribute(modelCmdType);

		CmdModelInfo info = new() {
			ModelType = modelCmdType,
			ModelHasConstructorWithParameters = hasCtorWithParams,
			CommandAttr = cmdAttr,
			ParseResultSetter = parseResultSetter,
		};

		object modelInst = info.GetModelInstance();

		CmdProp[] cmdProps = [.. props.Select(pp => CmdPropGetter.PropToCmpProp(modelCmdType, pp, modelInst)).Where(v => v != null)];
		info.Props = cmdProps;

		(MethodInfo method, bool handleIsAsync) = GetHandleMethod(modelCmdType);
		if(method != null)
			info.SetHandle(method, handleIsAsync);

		if(cmd == null)
			cmd = info.GetCommand();
		else
			info.SetCommand(cmd);

		return info;
	}

	public static bool ModelHasAnyConstructorsWithParameters([DynamicallyAccessedMembers(ModelMembers)] Type modelType)
		=> modelType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Any(ctor => ctor.GetParameters().Length > 0);

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

	/// <summary>
	/// For a property marked to get its <see cref="Option{T}"/> (or <see cref="Argument{T}"/>) from a static method:
	/// finds and invokes the static <c>Get{PropName}Option()</c>, <c>Get{PropName}Opt()</c> or <c>Get{PropName}()</c>
	/// (<c>Argument</c> / <c>Arg</c> for an argument) that takes no parameters and returns exactly <c>Option&lt;T&gt;</c>
	/// of the property's type. It looks on the command class and its base classes: the property's declaring class
	/// first, then the command class and on up. Null if there's none.
	/// </summary>
	/// <param name="modelType">The command (view) model type.</param>
	/// <param name="pi">The property.</param>
	/// <param name="isOption">True for an option, false for an argument.</param>
	public static object TryGetOptionFromStaticMethod(
		[DynamicallyAccessedMembers(ModelMembers)] Type modelType,
		PropertyInfo pi,
		bool isOption)
	{
		// the command class's public statics, its base classes' included
		MethodInfo[] staticMethods = modelType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);

		// the property's declaring class first, so what it offers is picked, as when only it was searched;
		// then the command class, and on up its base classes
		Type declaringType = pi.DeclaringType;
		List<Type> searchOrder = [declaringType];

		for(Type t = modelType; t != null; t = t.BaseType)
			if(t != declaringType)
				searchOrder.Add(t);

		foreach(Type t in searchOrder) {
			MethodInfo method = FindStaticOptionMethod(staticMethods.Where(m => m.DeclaringType == t), pi, isOption);
			if(method != null)
				return method.Invoke(null, null);
		}

		return null;
	}

	[RequiresUnreferencedCode("Looks for the static on the property's declaring type, which trimming can't follow. Use the overload that takes the command class's type.")]
	public static object TryGetOptionFromStaticMethod(PropertyInfo pi, bool isOption, Type assertExpectedType)
	{
#if EASYBUILDER_LEGACY
		// pre-AOT way, kept until the new lookup has run in the wild: the expected Option<T> made by MakeGenericType
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
#else
		Type declaringType = pi.DeclaringType;
		if(declaringType == null)
			return null;

		Type propertyType = pi.PropertyType;

		if(assertExpectedType != null && !IsOptionOrArgumentOf(assertExpectedType, isOption, propertyType))
			throw new ArgumentException($"Get static option or argument type is invalid for property '{pi.Name}' ({propertyType.FullName})");

		// statics of the property's declaring class only, as ever
		MethodInfo[] staticMethods = declaringType.GetMethods(BindingFlags.Static | BindingFlags.Public);

		MethodInfo method = FindStaticOptionMethod(staticMethods, pi, isOption);
		return method?.Invoke(null, null);
#endif
	}

	/// <summary>
	/// Of <paramref name="candidates"/>, the first that takes no parameters, returns exactly <c>Option&lt;T&gt;</c>
	/// (or <c>Argument&lt;T&gt;</c>) of the property's type, and is named, in this order, <c>Get{PropName}Option</c>,
	/// <c>Get{PropName}Opt</c> or <c>Get{PropName}</c> (<c>Argument</c> / <c>Arg</c> for an argument).
	/// </summary>
	static MethodInfo FindStaticOptionMethod(IEnumerable<MethodInfo> candidates, PropertyInfo pi, bool isOption)
	{
		string appdx1 = isOption ? "Option" : "Argument";
		string appdx2 = isOption ? "Opt" : "Arg";

		string propName = pi.Name;
		string[] methodNames = [$"Get{propName}{appdx1}", $"Get{propName}{appdx2}", $"Get{propName}"];

		MethodInfo[] matching = [.. candidates
			.Where(m => IsOptionOrArgumentOf(m.ReturnType, isOption, pi.PropertyType))
			.Where(m => m.GetParameters().Length == 0)];

		foreach(string methodNm in methodNames) {
			MethodInfo method = matching.FirstOrDefault(m => m.Name == methodNm);
			if(method != null)
				return method;
		}

		return null;
	}

	/// <summary>
	/// True if <paramref name="type"/> is exactly <c>Option&lt;T&gt;</c> (or <c>Argument&lt;T&gt;</c>) of
	/// <paramref name="valueType"/>, told from its generic definition and type argument, so no closed type
	/// has to be made to compare it against.
	/// </summary>
	static bool IsOptionOrArgumentOf(Type type, bool isOption, Type valueType)
		=> type.IsConstructedGenericType
		&& type.GetGenericTypeDefinition() == (isOption ? typeof(Option<>) : typeof(Argument<>))
		&& type.GenericTypeArguments[0] == valueType;

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
}

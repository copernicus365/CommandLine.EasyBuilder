using System.CommandLine;
using System.Reflection;

using CommandLine.EasyBuilder.Private;

namespace CommandLine.EasyBuilder.Auto;

public record AutoPropGroup(Type propertyType, bool isNullable, PropertyInfo pi, CommandLineValueAttribute attr, Option option, Argument argument, object defaultOfTVal)
{
	public bool IsOption => option != null;

	public bool IsNonNullableValueTypeAndValEqualsDefaultTButNotDefValue(object value)
	{
		bool val =
			propertyType.IsValueType &&
			!isNullable &&
			attr.DefVal != null &&
			value != null &&
			!value.Equals(attr.DefVal) &&
			value.Equals(defaultOfTVal);
		return val;
	}
}

public class AutoInfo
{
	public Type TType;

	public bool IsOption { get; private set; }

	public CommandAttribute Command;

	public AutoPropGroup[] Props;

	public bool NoProperties => Props.IsNulle();

	public MethodInfo Method { get; private set; }

	public object Handle;

	public bool HasHandle => Handle != null;

	public bool HandleIsAsync;

	public void SetHandle(MethodInfo mi, bool handleIsAsync, object handleDel)
	{
		HandleIsAsync = handleIsAsync;
		Method = mi;
		Handle = handleDel;
	}
}

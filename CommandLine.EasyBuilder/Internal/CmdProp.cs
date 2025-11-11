using System.CommandLine;
using System.Reflection;

namespace CommandLine.EasyBuilder.Internal;

public record CmdProp(Type type, bool isNullable, PropertyInfo pi, CLPropertyAttribute attr, Option option, Argument argument, object defaultOfTVal)
{
	public bool IsOption => option != null;

	public bool IsNonNullableValueTypeAndValEqualsDefaultTButNotDefValue(object value)
	{
		bool val =
			type.IsValueType &&
			!isNullable &&
			attr.DefVal != null &&
			value != null &&
			!value.Equals(attr.DefVal) &&
			value.Equals(defaultOfTVal);
		return val;
	}

	public void AddToCmd(Command cmd)
	{
		if(IsOption)
			cmd.Options.Add(option);
		else if(argument != null)
			cmd.Arguments.Add(argument);
	}
}

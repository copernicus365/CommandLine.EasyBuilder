using System.CommandLine;
using System.Reflection;

namespace CommandLine.EasyBuilder.Internal;

public record CmdProp(
	Type PType,
	bool IsNullable,
	PropertyInfo Prop,
	CLPropertyAttribute Attr,
	Option Opt,
	Argument Arg,
	object DefVal,
	object DefaultOfTVal)
{
	public bool IsOption => Opt != null;

	public bool IsArray { get; set; }

	public bool IsNumberArray { get; set; }

	public Type NumberArrayItemType { get; set; }

	/// <summary>
	/// The binder for <see cref="PType"/>, which made the option or argument and reads its value
	/// (null in a build with EASYBUILDER_LEGACY, whose old way reads it by reflection).
	/// </summary>
	public IPropTypeBinder Binder { get; set; }

	public bool IsNonNullableValueTypeAndValEqualsDefaultTButNotDefValue(object value)
	{
		bool val =
			PType.IsValueType &&
			!IsNullable &&
			DefVal != null &&
			value != null &&
			!value.Equals(DefVal) &&
			value.Equals(DefaultOfTVal);
		return val;
	}

	public void AddToCmd(Command cmd)
	{
		if(IsOption)
			cmd.Options.Add(Opt);
		else if(Arg != null)
			cmd.Arguments.Add(Arg);
	}
}

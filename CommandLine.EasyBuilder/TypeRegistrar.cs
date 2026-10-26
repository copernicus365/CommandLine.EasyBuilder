using CommandLine.EasyBuilder.Internal;

namespace CommandLine.EasyBuilder;

/// <summary>
/// Registers property types for EasyBuilder to bind with no reflection, as a chain from
/// <see cref="AutoBuilder.RegisterTypes"/>: <c>AutoBuilder.RegisterTypes().Add&lt;Point&gt;().AddEnum&lt;Color&gt;();</c>,
/// before the command tree is built. A native AOT build needs it for each property type beyond the built-in
/// ones (every type System.CommandLine parses on its own): an app's own enums, structs and classes. With dynamic
/// code on, any type binds without it.
/// </summary>
public readonly struct TypeRegistrar
{
	/// <summary>Registers exactly <typeparamref name="T"/>; its nullable form or its array is a step of its own.</summary>
	public TypeRegistrar Add<T>()
	{
		PropTypeBinders.Register<T>();
		return this;
	}

	/// <summary>Registers the enum <typeparamref name="TEnum"/>, its nullable form and its array.</summary>
	public TypeRegistrar AddEnum<TEnum>() where TEnum : struct, Enum
	{
		PropTypeBinders.Register<TEnum>();
		PropTypeBinders.Register<TEnum?>();
		PropTypeBinders.Register<TEnum[]>();
		return this;
	}
}

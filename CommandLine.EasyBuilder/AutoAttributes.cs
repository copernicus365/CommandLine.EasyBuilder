namespace CommandLine.EasyBuilder;

/// <summary>Command attribute.</summary>
/// <param name="name">Name</param>
/// <param name="description">Description</param>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class CommandAttribute(string name, string description = null)
	: CLNameAttribute(name, description: description)
{ }

/// <summary>Option attribute, with value.</summary>
/// <typeparam name="T"></typeparam>
/// <param name="name">Name</param>
/// <param name="alias">Alias</param>
/// <param name="description">Description</param>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class OptionAttribute<T>(string name, string alias = null, string description = null)
	: OptionAttribute(name, alias, description)
{ }

/// <summary>Option attribute, with value.</summary>
/// <param name="name">Name</param>
/// <param name="alias">Alias</param>
/// <param name="description">Description</param>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class OptionAttribute(string name, string alias = null, string description = null)
	: CLPropertyAttribute(name, alias, description)
{ }

/// <summary>Argument attribute.</summary>
/// <param name="name">Name</param>
/// <param name="description">Description</param>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class ArgumentAttribute(string name, string description = null)
	: CLPropertyAttribute(name, null, description)
{ }

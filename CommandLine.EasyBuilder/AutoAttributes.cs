namespace CommandLine.EasyBuilder;

/// <summary>Command attribute.</summary>
/// <param name="name">Name</param>
/// <param name="description">Description</param>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class CommandAttribute(string name, string description = null)
	: CLNameAttribute(name, description: description)
{ }

/// <summary>Option attribute, with value.</summary>
/// <param name="name">Name. Null or empty to use static method for getting Option or Argument</param>
/// <param name="alias">Alias</param>
/// <param name="description">Description</param>
/// <param name="required">Is required</param>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class OptionAttribute(string name, string alias = null, string description = null, bool required = false)
	: CLPropertyAttribute(name, alias, description, required)
{ }

/// <summary>Argument attribute.</summary>
/// <param name="name">Name</param>
/// <param name="description">Description</param>
/// <param name="required">Is required</param>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class ArgumentAttribute(string name, string description = null, bool required = false)
	: CLPropertyAttribute(name, null, description, required)
{ }

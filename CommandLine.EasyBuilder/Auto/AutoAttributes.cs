namespace CommandLine.EasyBuilder.Auto;

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class CommandAttribute(string name, string description = null)
	: CommandLineNameAttribute(name, description: description)
{ }

[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class OptionAttribute(string name, string alias = null, string description = null)
	: CommandLineValueAttribute(name, alias, description)
{ }

[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class ArgumentAttribute(string name, string description = null)
	: CommandLineValueAttribute(name, null, description)
{ }

[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public class HandleAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public class HandleAsyncAttribute : HandleAttribute { }

public class CommandLineValueAttribute : CommandLineNameAttribute
{
	public CommandLineValueAttribute() { }

	public CommandLineValueAttribute(string name, string alias = null, string description = null)
	{
		Name = name;
		Alias = alias;
		Description = description;
	}

	public bool Required { get; set; }

	public object DefVal { get; set; }
}

public abstract class CommandLineNameAttribute : Attribute
{
	public CommandLineNameAttribute() { }

	public CommandLineNameAttribute(string name, string alias = null, string description = null)
	{
		Name = name;
		Alias = alias;
		Description = description;
	}

	public string Name { get; set; }

	public string Alias { get; set; }

	public string Description { get; set; }
}

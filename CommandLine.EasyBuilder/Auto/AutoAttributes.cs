using System.CommandLine;

namespace CommandLine.EasyBuilder.Auto;

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class CommandAttribute(string name, string description = null)
	: CommandLineNameAttribute(name, description: description)
{ }

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

[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class OptionAttribute(string name, string alias = null, string description = null)
	: CommandLineValueAttribute(name, alias, description)
{ }

[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class ArgumentAttribute(string name, string description = null)
	: CommandLineValueAttribute(name, null, description)
{ }

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

	/// <summary>Gets or sets the name of the Option when displayed in help.</summary>
	public string HelpName { get; set; }

	///// <summary>Gets or sets the arity of the option.</summary>
	//public ArgumentArity Arity { get; set; }

	/// <summary>
	/// Gets a value that indicates whether multiple argument tokens are allowed for each option identifier token.
	/// </summary>
	/// <example>
	/// If set to <see langword="true"/>, the following command line is valid for passing multiple arguments:
	/// <code>
	/// > --opt 1 2 3
	/// </code>
	/// The following is equivalent and is always valid:
	/// <code>
	/// > --opt 1 --opt 2 --opt 3
	/// </code>
	/// </example>
	public bool AllowMultipleArgumentsPerToken { get; set; }

	//public string Extras { get; set; }

	//public Action DefaultValueFactory { get; set; }
	//public Func<System.CommandLine.Parsing.ArgumentResult, object> DefaultValueFactory { get; set; }

	//public static int DefaultValueFact(System.CommandLine.Parsing.ArgumentResult result);
	//public static Func<System.CommandLine.Parsing.ArgumentResult, T> DefaultValueFactory { get; set; }
}



//[AttributeUsage(AttributeTargets.Method, Inherited = true)]
//public class HandleAttribute : Attribute { }

//[AttributeUsage(AttributeTargets.Method, Inherited = true)]
//public class HandleAsyncAttribute : HandleAttribute { }

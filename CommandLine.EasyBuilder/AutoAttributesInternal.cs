namespace CommandLine.EasyBuilder;

public abstract class CLNameAttribute : Attribute
{
	public CLNameAttribute() { }

	public CLNameAttribute(string name, string alias = null, string description = null)
	{
		Name = name;
		Alias = alias;
		Description = description;
	}

	public string Name { get; set; }

	public string Alias { get; set; }

	public string Description { get; set; }
}

/// <summary>
/// Main attribute value but abstract to basically internal.
/// Current shortcoming: we use this internally to summarize both Option and Argument types.
/// But Argument shouldn't have a few of these properties. A problem to fix later, it's simplifying
/// to have a single type, but it does incorrectly expose some properties on Argument (a much less often
/// used thing).
/// </summary>
public abstract class CLPropertyAttribute : CLNameAttribute
{
	public CLPropertyAttribute() { }

	public CLPropertyAttribute(string name, string alias = null, string description = null, bool required = false)
	{
		Name = name;
		Alias = alias;
		Description = description;
		Required = required;
	}

	public bool Required { get; set; }

	public object DefVal { get; set; }

	/// <summary>Gets or sets the name of the Option when displayed in help.</summary>
	public string HelpName { get; set; }

	/// <summary>
	/// Gets a value that indicates whether multiple argument tokens are allowed for each option identifier token.
	/// </summary>
	public bool AllowMultipleArgumentsPerToken { get; set; }

	// unfortunately we can't use int? for attribute, so will always ignore if both are set to 0 (default, = not set)
	// if EITHER one is not zero, will use both

	/// <summary>Min arity value</summary>
	public int MinArity { get; set; }

	/// <summary>Max arity value</summary>
	public int MaxArity { get; set; }
}

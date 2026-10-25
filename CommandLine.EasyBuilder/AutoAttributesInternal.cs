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

	// an attribute can't take int?, so -1 (the default) means not set: neither set leaves System.CommandLine's
	// own arity for the type; if one is set, the other unset reads as 0. MaxArity = 0 makes a flag that takes no value

	/// <summary>Min arity value; -1 (the default) is not set.</summary>
	public int MinArity { get; set; } = -1;

	/// <summary>
	/// Max arity value; -1 (the default) is not set. 0 makes a flag that takes no value
	/// (<c>--apply</c>, never <c>--apply false</c>), which only a <c>bool</c> or <c>bool?</c> property can be.
	/// </summary>
	public int MaxArity { get; set; } = -1;
}

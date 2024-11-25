using System.CommandLine;

namespace CommandLine.EasyBuilder;

public static class BuilderFX
{
	// --- Alias ---

	public static Option<T> Alias<T>(this Option<T> opt, string alias)
	{
		opt.AddAlias(alias);
		return opt;
	}

	public static Command Alias(this Command cmd, string alias)
	{
		cmd.AddAlias(alias);
		return cmd;
	}


	// --- DefaultValue ---

	public static Option<T> DefaultValue<T>(this Option<T> opt, object? value)
	{
		opt.SetDefaultValue(value);
		return opt;
	}
}

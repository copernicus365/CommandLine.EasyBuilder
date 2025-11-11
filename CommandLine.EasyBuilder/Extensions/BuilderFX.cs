using System.CommandLine;

namespace CommandLine.EasyBuilder;

/// <summary>
/// Extension methods for functional building of Options/ Arguments.
/// </summary>
public static class BuilderFX
{
	// --- Alias ---

	public static Option<T> Alias<T>(this Option<T> opt, string alias)
	{
		opt.Aliases.Add(alias);
		return opt;
	}

	public static Option Alias(this Option opt, string alias)
	{
		opt.Aliases.Add(alias);
		return opt;
	}

	public static Command Alias(this Command cmd, string alias)
	{
		cmd.Aliases.Add(alias);
		return cmd;
	}

	// --- DefaultValue ---

	public static Option<T> DefaultValue<T>(this Option<T> opt, object? value)
	{
		opt.DefaultValueFactory = _ => (T)value!;
		//opt.SetDefaultValue(value);
		return opt;
	}
}

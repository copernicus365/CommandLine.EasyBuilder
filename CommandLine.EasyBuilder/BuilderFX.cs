using System.CommandLine;

using System.CommandLine.Binding;
using System.CommandLine.Parsing;

namespace CommandLine.EasyBuilder;

public static class BuilderFX
{
	public static Command Init<T1>(
		this Command cmd,
		IValueDescriptor<T1> symbol1,
		Action<T1> handle,
		Command parentCmd = null)
	{
		cmd
			.AddVal(symbol1);

		Handler.SetHandler(cmd, handle, symbol1);

		parentCmd?.AddCommand(cmd);
		return cmd;
	}

	public static Command Init<T1>(
		this Command cmd,
		IValueDescriptor<T1> symbol1,
		Func<T1, Task> handle,
		Command parentCmd = null)
	{
		cmd
			.AddVal(symbol1);

		Handler.SetHandler(cmd, handle, symbol1);

		parentCmd?.AddCommand(cmd);
		return cmd;
	}

	public static Command Init<T1, T2>(
		this Command cmd,
		IValueDescriptor<T1> symbol1,
		IValueDescriptor<T2> symbol2,
		Action<T1, T2> handle,
		Command parentCmd = null)
	{
		cmd
			.AddVal(symbol1)
			.AddVal(symbol2);

		Handler.SetHandler(cmd, handle, symbol1, symbol2);

		parentCmd?.AddCommand(cmd);
		return cmd;
	}

	public static Command Init<T1, T2>(
		this Command cmd,
		IValueDescriptor<T1> symbol1,
		IValueDescriptor<T2> symbol2,
		Func<T1, T2, Task> handle,
		Command parentCmd = null)
	{
		cmd
			.AddVal(symbol1)
			.AddVal(symbol2);

		Handler.SetHandler(cmd, handle, symbol1, symbol2);

		parentCmd?.AddCommand(cmd);
		return cmd;
	}

	public static Command Init<T1, T2, T3>(
		this Command cmd,
		IValueDescriptor<T1> symbol1,
		IValueDescriptor<T2> symbol2,
		IValueDescriptor<T3> symbol3,
		Action<T1, T2, T3> handle,
		Command parentCmd = null)
	{
		cmd
			.AddVal(symbol1)
			.AddVal(symbol2)
			.AddVal(symbol3);

		Handler.SetHandler(cmd, handle, symbol1, symbol2, symbol3);

		parentCmd?.AddCommand(cmd);
		return cmd;
	}

	public static Command Init<T1, T2, T3>(
		this Command cmd,
		IValueDescriptor<T1> symbol1,
		IValueDescriptor<T2> symbol2,
		IValueDescriptor<T3> symbol3,
		Func<T1, T2, T3, Task> handle,
		Command parentCmd = null)
	{
		cmd
			.AddVal(symbol1)
			.AddVal(symbol2)
			.AddVal(symbol3);

		Handler.SetHandler(cmd, handle, symbol1, symbol2, symbol3);

		parentCmd?.AddCommand(cmd);
		return cmd;
	}

	public static Command Init<T1, T2, T3, T4>(
		this Command cmd,
		IValueDescriptor<T1> symbol1,
		IValueDescriptor<T2> symbol2,
		IValueDescriptor<T3> symbol3,
		IValueDescriptor<T4> symbol4,
		Action<T1, T2, T3, T4> handle,
		Command parentCmd = null)
	{
		cmd
			.AddVal(symbol1)
			.AddVal(symbol2)
			.AddVal(symbol3)
			.AddVal(symbol4);

		Handler.SetHandler(cmd, handle, symbol1, symbol2, symbol3, symbol4);

		parentCmd?.AddCommand(cmd);
		return cmd;
	}

	/// <summary>
	/// Creates a new command, with handler based on a <see cref="Func{T1,T2,T3,T4,Task}"/>.
	/// If <paramref name="parentCmd"/> is not null, adds the new command to it.
	/// </summary>
	public static Command Init<T1, T2, T3, T4>(
		this Command cmd,
		IValueDescriptor<T1> symbol1,
		IValueDescriptor<T2> symbol2,
		IValueDescriptor<T3> symbol3,
		IValueDescriptor<T4> symbol4,
		Func<T1, T2, T3, T4, Task> handle,
		Command parentCmd = null)
	{
		cmd
			.AddVal(symbol1)
			.AddVal(symbol2)
			.AddVal(symbol3)
			.AddVal(symbol4);

		Handler.SetHandler(cmd, handle, symbol1, symbol2, symbol3, symbol4);

		parentCmd?.AddCommand(cmd);
		return cmd;
	}

	public static Command Init<T1, T2, T3, T4, T5>(
		this Command cmd,
		IValueDescriptor<T1> symbol1,
		IValueDescriptor<T2> symbol2,
		IValueDescriptor<T3> symbol3,
		IValueDescriptor<T4> symbol4,
		IValueDescriptor<T5> symbol5,
		Action<T1, T2, T3, T4, T5> handle,
		Command parentCmd = null)
	{
		cmd
			.AddVal(symbol1)
			.AddVal(symbol2)
			.AddVal(symbol3)
			.AddVal(symbol4)
			.AddVal(symbol5);

		Handler.SetHandler(cmd, handle, symbol1, symbol2, symbol3, symbol4, symbol5);

		parentCmd?.AddCommand(cmd);
		return cmd;
	}

	public static Command Init<T1, T2, T3, T4, T5>(
		this Command cmd,
		IValueDescriptor<T1> symbol1,
		IValueDescriptor<T2> symbol2,
		IValueDescriptor<T3> symbol3,
		IValueDescriptor<T4> symbol4,
		IValueDescriptor<T5> symbol5,
		Func<T1, T2, T3, T4, T5, Task> handle,
		Command parentCmd = null)
	{
		cmd
			.AddVal(symbol1)
			.AddVal(symbol2)
			.AddVal(symbol3)
			.AddVal(symbol4)
			.AddVal(symbol5);

		Handler.SetHandler(cmd, handle, symbol1, symbol2, symbol3, symbol4, symbol5);

		parentCmd?.AddCommand(cmd);
		return cmd;
	}

	public static Command Init<T1, T2, T3, T4, T5, T6>(
		this Command cmd,
		IValueDescriptor<T1> symbol1,
		IValueDescriptor<T2> symbol2,
		IValueDescriptor<T3> symbol3,
		IValueDescriptor<T4> symbol4,
		IValueDescriptor<T5> symbol5,
		IValueDescriptor<T6> symbol6,
		Action<T1, T2, T3, T4, T5, T6> handle,
		Command parentCmd = null)
	{
		cmd
			.AddVal(symbol1)
			.AddVal(symbol2)
			.AddVal(symbol3)
			.AddVal(symbol4)
			.AddVal(symbol5);

		Handler.SetHandler(cmd, handle, symbol1, symbol2, symbol3, symbol4, symbol5, symbol6);

		parentCmd?.AddCommand(cmd);
		return cmd;
	}


	public static Command Init<T1, T2, T3, T4, T5, T6>(
		this Command cmd,
		IValueDescriptor<T1> symbol1,
		IValueDescriptor<T2> symbol2,
		IValueDescriptor<T3> symbol3,
		IValueDescriptor<T4> symbol4,
		IValueDescriptor<T5> symbol5,
		IValueDescriptor<T6> symbol6,
		Func<T1, T2, T3, T4, T5, T6, Task> handle,
		Command parentCmd = null)
	{
		cmd
			.AddVal(symbol1)
			.AddVal(symbol2)
			.AddVal(symbol3)
			.AddVal(symbol4)
			.AddVal(symbol5);

		Handler.SetHandler(cmd, handle, symbol1, symbol2, symbol3, symbol4, symbol5, symbol6);

		parentCmd?.AddCommand(cmd);
		return cmd;
	}

	static Command AddVal<T>(this Command cmd, IValueDescriptor<T> val)
	{
		switch(val) {
			case Option<T> opt:
				cmd.AddOption(opt); break;
			case Argument<T> arg:
				cmd.AddArgument(arg); break;
			//case Command opt:
			//	cmd.AddOption(opt); return;
			default:
				throw new ArgumentOutOfRangeException(nameof(val));
		}
		return cmd;
	}

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

public static class ArgParsers
{
	/// <summary>
	/// Use in Option constructor set to `parseArgument`,
	/// for parsing a string value that should be parsed as an
	/// integer array.
	/// </summary>
	/// <param name="arg">Arg</param>
	public static int[] IntArray(ArgumentResult arg)
	=> ArrayParser(arg, str => int.TryParse(str, out int val) ? (int?)val : null);

	public static double[] DoubleArray(ArgumentResult arg)
		=> ArrayParser(arg, str => double.TryParse(str, out double val) ? (double?)val : null);

	public static long[] LongArray(ArgumentResult arg)
		=> ArrayParser(arg, str => long.TryParse(str, out long val) ? (long?)val : null);

	public static decimal[] DecimalArray(ArgumentResult arg)
		=> ArrayParser(arg, str => decimal.TryParse(str, out decimal val) ? (decimal?)val : null);

	public static T[] ArrayParser<T>(ArgumentResult arg, Func<string, T?> conv) where T : struct
	{
		string str = arg.Tokens.Single().Value;//?.Trim();
		if(str == null)
			return null;

		// It turns out "quotes" are already removed by input framework, so no use striping them

		if(str.Length > 1 && str[0] == '[')
			str = str[1..^1];

		string[] svals = str
			.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		T[] arr = new T[svals.Length];

		for(int i = 0; i < svals.Length; i++) {
			string s = svals[i];
			T? val = conv(s);
			if(val == null) {
				arg.ErrorMessage = $"Value is out of range or invalid: {s}";
				return null;
			}
			arr[i] = val.Value;
		}

		return arr;
	}
}

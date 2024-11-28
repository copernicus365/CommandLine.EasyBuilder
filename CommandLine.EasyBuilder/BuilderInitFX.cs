using System.CommandLine;

using System.CommandLine.Binding;

namespace CommandLine.EasyBuilder;

public static class BuilderInitFX
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
			.AddVal(symbol5)
			.AddVal(symbol6)
			;

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
			.AddVal(symbol5)
			.AddVal(symbol6)
			;

		Handler.SetHandler(cmd, handle, symbol1, symbol2, symbol3, symbol4, symbol5, symbol6);

		parentCmd?.AddCommand(cmd);
		return cmd;
	}

	public static Command Init<T1, T2, T3, T4, T5, T6, T7>(
		this Command cmd,
		IValueDescriptor<T1> symbol1,
		IValueDescriptor<T2> symbol2,
		IValueDescriptor<T3> symbol3,
		IValueDescriptor<T4> symbol4,
		IValueDescriptor<T5> symbol5,
		IValueDescriptor<T6> symbol6,
		IValueDescriptor<T7> symbol7,
		Action<T1, T2, T3, T4, T5, T6, T7> handle,
		Command parentCmd = null)
	{
		cmd
			.AddVal(symbol1)
			.AddVal(symbol2)
			.AddVal(symbol3)
			.AddVal(symbol4)
			.AddVal(symbol5)
			.AddVal(symbol6)
			.AddVal(symbol7)
			;

		Handler.SetHandler(cmd, handle, symbol1, symbol2, symbol3, symbol4, symbol5, symbol6, symbol7);

		parentCmd?.AddCommand(cmd);
		return cmd;
	}

	public static Command Init<T1, T2, T3, T4, T5, T6, T7>(
		this Command cmd,
		IValueDescriptor<T1> symbol1,
		IValueDescriptor<T2> symbol2,
		IValueDescriptor<T3> symbol3,
		IValueDescriptor<T4> symbol4,
		IValueDescriptor<T5> symbol5,
		IValueDescriptor<T6> symbol6,
		IValueDescriptor<T7> symbol7,
		Func<T1, T2, T3, T4, T5, T6, T7, Task> handle,
		Command parentCmd = null)
	{
		cmd
			.AddVal(symbol1)
			.AddVal(symbol2)
			.AddVal(symbol3)
			.AddVal(symbol4)
			.AddVal(symbol5)
			.AddVal(symbol6)
			.AddVal(symbol7)
			;

		Handler.SetHandler(cmd, handle, symbol1, symbol2, symbol3, symbol4, symbol5, symbol6, symbol7);

		parentCmd?.AddCommand(cmd);
		return cmd;
	}

	public static Command Init<T1, T2, T3, T4, T5, T6, T7, T8>(
		this Command cmd,
		IValueDescriptor<T1> symbol1,
		IValueDescriptor<T2> symbol2,
		IValueDescriptor<T3> symbol3,
		IValueDescriptor<T4> symbol4,
		IValueDescriptor<T5> symbol5,
		IValueDescriptor<T6> symbol6,
		IValueDescriptor<T7> symbol7,
		IValueDescriptor<T8> symbol8,
		Action<T1, T2, T3, T4, T5, T6, T7, T8> handle,
		Command parentCmd = null)
	{
		cmd
			.AddVal(symbol1)
			.AddVal(symbol2)
			.AddVal(symbol3)
			.AddVal(symbol4)
			.AddVal(symbol5)
			.AddVal(symbol6)
			.AddVal(symbol7)
			.AddVal(symbol8)
			;

		Handler.SetHandler(cmd, handle, symbol1, symbol2, symbol3, symbol4, symbol5, symbol6, symbol7, symbol8);

		parentCmd?.AddCommand(cmd);
		return cmd;
	}

	public static Command Init<T1, T2, T3, T4, T5, T6, T7, T8>(
		this Command cmd,
		IValueDescriptor<T1> symbol1,
		IValueDescriptor<T2> symbol2,
		IValueDescriptor<T3> symbol3,
		IValueDescriptor<T4> symbol4,
		IValueDescriptor<T5> symbol5,
		IValueDescriptor<T6> symbol6,
		IValueDescriptor<T7> symbol7,
		IValueDescriptor<T8> symbol8,
		Func<T1, T2, T3, T4, T5, T6, T7, T8, Task> handle,
		Command parentCmd = null)
	{
		cmd
			.AddVal(symbol1)
			.AddVal(symbol2)
			.AddVal(symbol3)
			.AddVal(symbol4)
			.AddVal(symbol5)
			.AddVal(symbol6)
			.AddVal(symbol7)
			.AddVal(symbol8)
			;

		Handler.SetHandler(cmd, handle, symbol1, symbol2, symbol3, symbol4, symbol5, symbol6, symbol7, symbol8);

		parentCmd?.AddCommand(cmd);
		return cmd;
	}

	public static Command AddVal<T>(this Command cmd, IValueDescriptor<T> val)
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
}

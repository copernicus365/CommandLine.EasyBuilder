using System.CommandLine;

using System.CommandLine.Binding;

using CommandLine.EasyBuilder.Auto;

namespace CommandLine.EasyBuilder;

public static class AutoBuilderFX
{
	public static Command AddAutoCommand<TAuto>(this Command parentCmd) where TAuto : class, new()
	{
		Command cmd = AddAuto(parentCmd, out AutoAttributesBinder<TAuto> binder, setHandle: true);

		AutoInfo info = binder.Info;

		if(!info.HandleIsAsync)
			Handler.SetHandler(cmd, info.Handle as Action<TAuto>, binder);
		else
			Handler.SetHandler(cmd, info.Handle as Func<TAuto, Task>, binder);
		return cmd;
	}

	public static Command AddAutoCommand<TAuto>(
		this Command parentCmd,
		Action<TAuto> handle) where TAuto : class, new()
	{
		Command cmd = AddAuto(parentCmd, out AutoAttributesBinder<TAuto> binder);
		Handler.SetHandler(cmd, handle, binder);
		return cmd;
	}

	public static Command AddAutoCommand<TAuto>(
		this Command parentCmd,
		Func<TAuto, Task> handle) where TAuto : class, new()
	{
		Command cmd = AddAuto(parentCmd, out AutoAttributesBinder<TAuto> binder);
		Handler.SetHandler(cmd, handle, binder);
		return cmd;
	}

	public static Command AddAutoCommand<TAuto, T2>(
		this Command parentCmd,
		IValueDescriptor<T2> symbol2,
		Action<TAuto, T2> handle) where TAuto : class, new()
	{
		Command cmd = AddAuto(parentCmd, out AutoAttributesBinder<TAuto> binder);
		cmd.AddVal(symbol2);

		Handler.SetHandler(cmd, handle, binder, symbol2);
		return cmd;
	}

	public static Command AddAutoCommand<TAuto, T2>(
		this Command parentCmd,
		IValueDescriptor<T2> symbol2,
		Func<TAuto, T2, Task> handle) where TAuto : class, new()
	{
		Command cmd = AddAuto(parentCmd, out AutoAttributesBinder<TAuto> binder);
		cmd.AddVal(symbol2);

		Handler.SetHandler(cmd, handle, binder, symbol2);
		return cmd;
	}

	public static Command AddAutoCommand<TAuto, T2, T3>(
		this Command parentCmd,
		IValueDescriptor<T2> symbol2,
		IValueDescriptor<T3> symbol3,
		Action<TAuto, T2, T3> handle) where TAuto : class, new()
	{
		Command cmd = AddAuto(parentCmd, out AutoAttributesBinder<TAuto> binder);
		cmd.AddVal(symbol2);
		cmd.AddVal(symbol3);

		Handler.SetHandler(cmd, handle, binder, symbol2, symbol3);
		return cmd;
	}

	public static Command AddAutoCommand<TAuto, T2, T3>(
		this Command parentCmd,
		IValueDescriptor<T2> symbol2,
		IValueDescriptor<T3> symbol3,
		Func<TAuto, T2, T3, Task> handle) where TAuto : class, new()
	{
		Command cmd = AddAuto(parentCmd, out AutoAttributesBinder<TAuto> binder);
		cmd.AddVal(symbol2);
		cmd.AddVal(symbol3);

		Handler.SetHandler(cmd, handle, binder, symbol2, symbol3);
		return cmd;
	}


	static Command AddAuto<T>(Command parentCmd, out AutoAttributesBinder<T> binder, bool setHandle = false) where T : class, new()
	{
		AutoInfo autoInfo = OptionsClassConverter.GetAutoInfoOrThrow<T>();
		binder = new(autoInfo);

		AutoPropGroup[] arr = autoInfo.Props;
		CommandAttribute cmdAttr = autoInfo.Command;

		Command cmd = new(name: cmdAttr.Name, description: cmdAttr.Description);

		if(cmdAttr.Alias != null)
			cmd.Alias(cmdAttr.Alias);

		foreach(AutoPropGroup p in arr)
			AddVal(cmd, p.IsOption ? p.option : p.argument);

		parentCmd?.AddCommand(cmd);

		if(setHandle) {
			object handle = OptionsClassConverter.GetHandleMethod<T>(autoInfo);
			if(!autoInfo.HasHandle) {
				// actually can be handy to have simple commands with no handle
				//throw new ArgumentException("Type has no handle");
			}
		}
		return cmd;
	}

	static Command AddVal(this Command cmd, IValueDescriptor val)
	{
		switch(val) {
			case Option opt:
				cmd.AddOption(opt); break;
			case Argument arg:
				cmd.AddArgument(arg); break;
			//case Command opt:
			//	cmd.AddOption(opt); return;
			default:
				throw new ArgumentOutOfRangeException(nameof(val));
		}
		return cmd;
	}
}

//public class AutoBuilder(RootCommand rootCmd)
//{
//}

//using System.CommandLine;

//using System.CommandLine.Binding;

//using CommandLine.EasyBuilder.Auto;

//namespace CommandLine.EasyBuilder;

//public static class AutoBuilderFX
//{
//	/// <summary>
//	/// Adds `TAuto` as a child <see cref="Command"/>,
//	/// but then *returns the parent* command. USE this when you do NOT need
//	/// an instance of the newly created <see cref="Command"/>. What this makes for
//	/// is a very handy and functional chained build process of adding often multiple
//	/// commands to a parent all in one call chain.
//	/// </summary>
//	/// <typeparam name="TAuto"></typeparam>
//	/// <param name="parentCmd"></param>
//	/// <returns>Returns the PARENT, not the new command</returns>
//	public static Command Auto<TAuto>(this Command parentCmd) where TAuto : class, new()
//	{
//		var cmd = parentCmd.AddAutoCommand<TAuto>();
//		return parentCmd;
//	}


//	/// <summary>
//	/// Makes an instance of `TAuto` as a child <see cref="Command"/> added to
//	/// the parent. Then returns the newly created child instance.
//	/// </summary>
//	/// <typeparam name="TAuto"></typeparam>
//	/// <param name="parentCmd"></param>
//	/// <returns>Returns new instance of `TAuto` just added to parent</returns>
//	public static Command AddAutoCommand<TAuto>(this Command parentCmd) where TAuto : class, new()
//	{
//		Command cmd = AddAuto(parentCmd, out AutoAttributesBinder<TAuto> binder, setHandle: true);

//		AutoInfo info = binder.Info;

//		if(!info.HandleIsAsync)
//			Handler.SetHandler(cmd, info.Handle as Action<TAuto>, binder);
//		else
//			Handler.SetHandler(cmd, info.Handle as Func<TAuto, Task>, binder);
//		return cmd;
//	}

//	public static Command AddAutoCommand<TAuto>(
//		this Command parentCmd,
//		Action<TAuto> handle) where TAuto : class, new()
//	{
//		Command cmd = AddAuto(parentCmd, out AutoAttributesBinder<TAuto> binder);
//		Handler.SetHandler(cmd, handle, binder);
//		return cmd;
//	}

//	public static Command AddAutoCommand<TAuto>(
//		this Command parentCmd,
//		Func<TAuto, Task> handle) where TAuto : class, new()
//	{
//		Command cmd = AddAuto(parentCmd, out AutoAttributesBinder<TAuto> binder);
//		Handler.SetHandler(cmd, handle, binder);
//		return cmd;
//	}

//	public static Command AddAutoCommand<TAuto, T2>(
//		this Command parentCmd,
//		IValueDescriptor<T2> symbol2,
//		Action<TAuto, T2> handle) where TAuto : class, new()
//	{
//		Command cmd = AddAuto(parentCmd, out AutoAttributesBinder<TAuto> binder);
//		cmd.AddVal(symbol2);

//		Handler.SetHandler(cmd, handle, binder, symbol2);
//		return cmd;
//	}

//	public static Command AddAutoCommand<TAuto, T2>(
//		this Command parentCmd,
//		IValueDescriptor<T2> symbol2,
//		Func<TAuto, T2, Task> handle) where TAuto : class, new()
//	{
//		Command cmd = AddAuto(parentCmd, out AutoAttributesBinder<TAuto> binder);
//		cmd.AddVal(symbol2);

//		Handler.SetHandler(cmd, handle, binder, symbol2);
//		return cmd;
//	}

//	public static Command AddAutoCommand<TAuto, T2, T3>(
//		this Command parentCmd,
//		IValueDescriptor<T2> symbol2,
//		IValueDescriptor<T3> symbol3,
//		Action<TAuto, T2, T3> handle) where TAuto : class, new()
//	{
//		Command cmd = AddAuto(parentCmd, out AutoAttributesBinder<TAuto> binder);
//		cmd.AddVal(symbol2);
//		cmd.AddVal(symbol3);

//		Handler.SetHandler(cmd, handle, binder, symbol2, symbol3);
//		return cmd;
//	}

//	public static Command AddAutoCommand<TAuto, T2, T3>(
//		this Command parentCmd,
//		IValueDescriptor<T2> symbol2,
//		IValueDescriptor<T3> symbol3,
//		Func<TAuto, T2, T3, Task> handle) where TAuto : class, new()
//	{
//		Command cmd = AddAuto(parentCmd, out AutoAttributesBinder<TAuto> binder);
//		cmd.AddVal(symbol2);
//		cmd.AddVal(symbol3);

//		Handler.SetHandler(cmd, handle, binder, symbol2, symbol3);
//		return cmd;
//	}


//	static Command AddAuto<T>(Command parentCmd, out AutoAttributesBinder<T> binder, bool setHandle = false) where T : class, new()
//	{
//		AutoInfo autoInfo = OptionsClassConverter.GetAutoInfoOrThrow<T>();
//		binder = new(autoInfo);

//		AutoPropGroup[] arr = autoInfo.Props;
//		CommandAttribute cmdAttr = autoInfo.Command;

//		Command cmd = new(name: cmdAttr.Name, description: cmdAttr.Description);

//		if(cmdAttr.Alias != null)
//			cmd.Alias(cmdAttr.Alias);

//		foreach(AutoPropGroup p in arr)
//			AddVal(cmd, p.IsOption ? p.option : p.argument);

//		parentCmd?.AddCommand(cmd);

//		if(setHandle) {
//			object handle = OptionsClassConverter.GetHandleMethod<T>(autoInfo);
//			if(!autoInfo.HasHandle) {
//				// actually can be handy to have simple commands with no handle
//				//throw new ArgumentException("Type has no handle");
//			}
//		}
//		return cmd;
//	}

//	static Command AddVal(this Command cmd, IValueDescriptor val)
//	{
//		Option<string> bbb;

//		switch(val) {
//			case Option opt:
//				cmd.AddOption(opt); break;
//			case Argument arg:
//				cmd.AddArgument(arg); break;
//			//case Command opt:
//			//	cmd.AddOption(opt); return;
//			default:
//				throw new ArgumentOutOfRangeException(nameof(val));
//		} System.CommandLine.
//		return cmd;
//	}
//}

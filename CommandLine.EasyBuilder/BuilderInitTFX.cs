using System.CommandLine;

using System.CommandLine.Binding;

using CommandLine.EasyBuilder.Auto;

namespace CommandLine.EasyBuilder;

public static class BuilderInitTFX
{
	public static Command AddCommand<T>(
		this Command parentCmd,
		Action<T> handle) where T : class, new()
	{
		AutoAttributesBinder<T> binder = new();

		AutoPropGroup[] arr = binder.Props;
		ControlAttribute cntAttr = binder.ControlAttr;

		Command cmd = new(name: cntAttr.Name, description: cntAttr.Description);

		if(cntAttr.Alias != null) {
			cmd.Alias(cntAttr.Alias);

			if(cntAttr.Alias2 != null)
				cmd.Alias(cntAttr.Alias2);
		}

		foreach(AutoPropGroup p in arr)
			AddVal(cmd, p.option);

		Handler.SetHandler(cmd, handle, binder);

		parentCmd?.AddCommand(cmd);
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

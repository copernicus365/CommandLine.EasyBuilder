using System.CommandLine;

using System.CommandLine.Binding;

using CommandLine.EasyBuilder.Auto;

namespace CommandLine.EasyBuilder;

public static class BuilderInitTFX
{
	public static Command InitCommand<T>(
		this T control, //IValueDescriptor<T> symbol1,
		Action<T> handle,
		Command parentCmd = null) where T : class, IControl, new()
	{
		ControlAttribute cattr = OptionsClassConverter.GetControlAttr2(control)
			?? throw new ArgumentException();

		Command cmd = new(name: cattr.Name, description: cattr.Description);

		if(cattr.Alias != null) {
			cmd.Alias(cattr.Alias);

			if(cattr.Alias2 != null)
				cmd.Alias(cattr.Alias2);
		}

		OptionsAttrBinder<T> binder = new();

		OptPropGroup[] arr = binder.Props;

		foreach(OptPropGroup p in arr)
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

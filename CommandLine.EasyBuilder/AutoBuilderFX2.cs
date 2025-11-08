using System.CommandLine;

using CommandLine.EasyBuilder.Auto;

namespace CommandLine.EasyBuilder;

public static class AutoBuilderFX
{
	/// <summary>
	/// Makes an instance of `TAuto` as a child <see cref="Command"/> added to
	/// the parent. Then returns the newly created child instance.
	/// </summary>
	/// <typeparam name="TAuto"></typeparam>
	/// <param name="parentCmd"></param>
	/// <returns>Returns new instance of `TAuto` just added to parent</returns>
	public static Command AddAutoCommand<TAuto>(this Command parentCmd) where TAuto : class, new()
	{
		Command cmd = AddAuto<TAuto>(parentCmd);
		return cmd;
	}

	static Command AddAuto<T>(Command parentCmd) where T : class, new()
	{
		CmdModelInfo<T> model = CmdModelReflectionHelper.GetCmdModelInfo<T>();

		CmdProp[] arr = model.Props;
		CommandAttribute cmdAttr = model.CommandAttr;


		Command cmd = new(name: cmdAttr.Name, description: cmdAttr.Description);
		model.Cmd = cmd;

		if(cmdAttr.Alias != null)
			cmd.Alias(cmdAttr.Alias);

		foreach(CmdProp p in arr)
			p.AddToCmd(cmd); //AddVal(cmd, p.IsOption ? p.option : p.argument);

		parentCmd?.Subcommands.Add(cmd);

		CmdModelReflectionHelper.SetHandleMethod(model);

		model.SetCommandHandler();

		return cmd;
	}
}

using System.CommandLine;

using CommandLine.EasyBuilder.Internal;

namespace CommandLine.EasyBuilder;

public static class AutoBuilderFX
{
	/// <summary>
	/// Makes an instance of `TAuto` as a child <see cref="Command"/> added to
	/// the parent. Then returns the newly created child instance.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	/// <param name="parentCmd"></param>
	/// <returns>Returns new instance of `TAuto` just added to parent</returns>
	public static Command AddAutoCommand<TModel>(this Command parentCmd) //where TModel : class, new()
		=> AddAuto(typeof(TModel), parentCmd);

	public static Command AddAutoCommand(this Command parentCmd, Type modelCmdType)
		=> AddAuto(modelCmdType, parentCmd);

	static Command AddAuto(Type modelCmdType, Command parentCmd)
	{
		CmdModelInfo model = CmdModelReflectionHelper.GetCmdModelInfo(modelCmdType);
		Command cmd = model.Cmd;
		parentCmd?.Subcommands.Add(cmd);
		return cmd;
	}


	public static Command SetAutoCommand<TModel>(this Command cmd) //where TModel : class, new()
		=> SetAuto(typeof(TModel), cmd);


	static Command SetAuto(Type modelCmdType, Command cmd)
	{
		ArgumentNullException.ThrowIfNull(cmd);

		CmdModelInfo model = CmdModelReflectionHelper.GetCmdModelInfo(modelCmdType, cmd: cmd);
		if(model.Cmd == null || model.Cmd != cmd)
			throw new InvalidOperationException("Command instance should match provided Command.");
		return cmd;
	}
}

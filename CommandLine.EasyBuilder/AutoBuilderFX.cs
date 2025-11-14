using System.CommandLine;

using CommandLine.EasyBuilder.Internal;

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
		=> AddAuto(typeof(TAuto), parentCmd);

	public static Command AddAutoCommand(this Command parentCmd, Type cmdType)
		=> AddAuto(cmdType, parentCmd);

	static Command AddAuto(Type cmdType, Command parentCmd) //where T : class, new()
	{
		CmdModelInfo model = CmdModelReflectionHelper.GetCmdModelInfo(cmdType);
		Command cmd = model.Cmd;
		parentCmd?.Subcommands.Add(cmd);
		return cmd;
	}
}

using System.CommandLine;
using System.Diagnostics.CodeAnalysis;

using CommandLine.EasyBuilder.Internal;

namespace CommandLine.EasyBuilder;

public static class AutoBuilder
{
	/// <summary>
	/// Makes an instance of `TAuto` as a child <see cref="Command"/> added to
	/// the parent. Then returns the newly created child instance.
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	/// <param name="parentCmd"></param>
	/// <returns>Returns new instance of `TAuto` just added to parent</returns>
	public static Command AddAutoCommand<[DynamicallyAccessedMembers(CmdModelReflectionHelper.ModelMembers)] TModel>(this Command parentCmd)
		=> AddAuto(typeof(TModel), parentCmd);

	public static Command AddAutoCommand(this Command parentCmd, [DynamicallyAccessedMembers(CmdModelReflectionHelper.ModelMembers)] Type modelCmdType)
		=> AddAuto(modelCmdType, parentCmd);

	static Command AddAuto([DynamicallyAccessedMembers(CmdModelReflectionHelper.ModelMembers)] Type modelCmdType, Command parentCmd)
	{
		CmdModelInfo model = CmdModelReflectionHelper.GetCmdModelInfo(modelCmdType);
		Command cmd = model.Cmd;
		parentCmd?.Subcommands.Add(cmd);
		return cmd;
	}


	public static Command SetAutoCommand<[DynamicallyAccessedMembers(CmdModelReflectionHelper.ModelMembers)] TModel>(this Command cmd)
		=> SetAuto(typeof(TModel), cmd);


	static Command SetAuto([DynamicallyAccessedMembers(CmdModelReflectionHelper.ModelMembers)] Type modelCmdType, Command cmd)
	{
		ArgumentNullException.ThrowIfNull(cmd);

		CmdModelInfo model = CmdModelReflectionHelper.GetCmdModelInfo(modelCmdType, cmd: cmd);
		if(model.Cmd == null || model.Cmd != cmd)
			throw new InvalidOperationException("Command instance should match provided Command.");
		return cmd;
	}


	/// <summary>
	/// Starts a chain registering the property types a native AOT build can't bind on its own, called before the
	/// command tree is built: <c>AutoBuilder.RegisterTypes().Add&lt;Point&gt;().AddEnum&lt;Color&gt;();</c>.
	/// See <see cref="TypeRegistrar"/>.
	/// </summary>
	public static TypeRegistrar RegisterTypes() => new();
}

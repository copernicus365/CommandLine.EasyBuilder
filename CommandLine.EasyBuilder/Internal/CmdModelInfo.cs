using System.CommandLine;
using System.Reflection;

using CommandLine.EasyBuilder.Private;

namespace CommandLine.EasyBuilder.Internal;

public abstract class CmdModelInfo
{
	/// <summary>The command (view) model type</summary>
	public Type Type;

	// ???? what the heck was this?? -> public bool IsOption { get; private set; }

	public Command Cmd;

	/// <summary>
	/// What makes a command (view) model is being a class set with this attribute.
	/// This is the instance of that cmd attribute.
	/// </summary>
	public CommandAttribute CommandAttr;

	/// <summary>The properties set on this cmd-model.</summary>
	public CmdProp[] Props;

	/// <summary>True if not props exist. There are good reasons to allow that</summary>
	public bool NoProperties => Props.IsNulle();

	public MethodInfo Method { get; private set; }

	public bool HasParseResultProp => ParseResultSetter != null;

	public MethodInfo ParseResultSetter { get; set; }

	public object Handle;

	public bool HasHandle => Handle != null;

	public bool HandleIsAsync;

	public void SetHandle(MethodInfo mi, bool handleIsAsync)
	{
		HandleIsAsync = handleIsAsync;
		Method = mi;
	}

	public void SetHandle(object handleDelegate)
		=> Handle = handleDelegate;
}

public class CmdModelInfo<TAuto> : CmdModelInfo where TAuto : new()
{
	public CmdModelInfo() { }

	public TAuto GetModelInstanceAndPopulateValues(ParseResult parseRes)
	{
		//public TAuto GetBoundValue(ParseResult parseRes) //BindingContext bindingContext)
		//ParseResult parseRes = bindingContext.ParseResult;
		TAuto cmdModel = new();

		var props = Props;
		for(int i = 0; i < props.Length; i++) {
			CmdProp apg = props[i];
			CmdModelReflectionHelper.SetVal(parseRes, apg, cmdModel);
		}

		if(HasParseResultProp)
			ParseResultSetter.Invoke(cmdModel, [parseRes]);

		// LATER: make a check on TAuto if it has a prop named `ParsedResult` of this type,
		// IF SO SET that property!! that way no need to send it in to handler and great simplication / number of handlers!
		return cmdModel;
	}

	public void SetCommandHandler()
	{
		if(!HandleIsAsync) {
			Cmd.SetAction(r => {
				TAuto item = GetModelInstanceAndPopulateValues(r);
				Method.Invoke(item, null);
			});
		}
		else {
			Func<ParseResult, Task> action = async r => {
				TAuto item = GetModelInstanceAndPopulateValues(r);
				await (Task)Method.Invoke(item, null);
			};

			Cmd.SetAction(action);
		}
	}
}

using System.CommandLine;
using System.Reflection;

using CommandLine.EasyBuilder.Private;

namespace CommandLine.EasyBuilder.Auto;

public record CmdProp(Type type, bool isNullable, PropertyInfo pi, CommandLineValueAttribute attr, Option option, Argument argument, object defaultOfTVal)
{
	public bool IsOption => option != null;

	public bool IsNonNullableValueTypeAndValEqualsDefaultTButNotDefValue(object value)
	{
		bool val =
			type.IsValueType &&
			!isNullable &&
			attr.DefVal != null &&
			value != null &&
			!value.Equals(attr.DefVal) &&
			value.Equals(defaultOfTVal);
		return val;
	}

	public void AddToCmd(Command cmd)
	{
		if(IsOption)
			cmd.Options.Add(option);
		else if(argument != null)
			cmd.Arguments.Add(argument);
	}
}

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

	public TAuto GetInstance(ParseResult parseRes)
	{
		//public TAuto GetBoundValue(ParseResult parseRes) //BindingContext bindingContext)
		//ParseResult parseRes = bindingContext.ParseResult;
		TAuto cmdModel = new();

		var props = Props;
		for(int i = 0; i < props.Length; i++) {
			CmdProp apg = props[i];
			CmdModelReflectionHelper.SetVal(parseRes, apg, cmdModel);
		}

		// LATER: make a check on TAuto if it has a prop named `ParsedResult` of this type,
		// IF SO SET that property!! that way no need to send it in to handler and great simplication / number of handlers!
		return cmdModel;
	}

	public void SetCommandHandler()
	{
		if(!HandleIsAsync) {
			Cmd.SetAction(r => {
				TAuto item = GetInstance(r);
				Method.Invoke(item, null);
			});
		}
	}
}

//if(setHandle) {
//	object handle = OptionsClassConverter.SetHandleMethod<T>(info, cmd);
//	if(!info.HasHandle) {
//		// actually can be handy to have simple commands with no handle
//		//throw new ArgumentException("Type has no handle");
//	}
//}

//public class AutoBinder<TAuto>(CmdModelInfo autoInfo, Command cmd) where TAuto : class, new() //BinderBase<T> where T : class, new()
//{
//	public readonly CmdModelInfo Info = autoInfo;
//	public readonly Command Cmd = cmd;

//	public TAuto GetBoundValue(ParseResult parseRes) //BindingContext bindingContext)
//	{
//		TAuto item = new();

//		//ParseResult parseRes = bindingContext.ParseResult;

//		var props = Info.Props;
//		for(int i = 0; i < props.Length; i++) {
//			CmdProp apg = props[i];
//			CmdModelReflectionHelper.SetVal(parseRes, apg, item);
//		}

//		return item;
//	}
//}

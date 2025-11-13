using System.CommandLine;
using System.Reflection;

namespace CommandLine.EasyBuilder.Internal;

/// <summary>
/// This type wraps a class with CommandAttribute applied to it, and herein will
/// be all its auto properties. This ends up being the glue that persists from app
/// creation, along with an instance of the CommandAttribute that applied to the model
/// class.
/// NOTE: The model class itself is NOT created here, that is an instance type created
/// every time things run, so it can't persist any of this data. It does seem confusing
/// since all of these attributes are *specified* (ie statically) on the model class, even
/// though *instances* of the model class are created every time the cline runs. Anyways,
/// the reflection business ultimately builds up and saves the key data in an instance
/// of this type. 
/// </summary>
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
	public bool NoProperties => (Props?.Length ?? 0) < 1;

	public MethodInfo Method { get; private set; }

	public bool HasParseResultProp => ParseResultSetter != null;

	public MethodInfo ParseResultSetter { get; set; }

	public object Handle;

	public bool HandleIsAsync;

	public void SetHandle(MethodInfo mi, bool handleIsAsync)
	{
		HandleIsAsync = handleIsAsync;
		Method = mi;
	}
}

/// <summary>
/// A generic version of <see cref="CmdModelInfo"/> above, but adding a generic model type,
/// representing the type of model that our command was specified on with its props.
/// </summary>
/// <typeparam name="TModel">Model type</typeparam>
public class CmdModelInfo<TModel> : CmdModelInfo where TModel : new()
{
	public CmdModelInfo() { }

	/// <summary>
	/// This gets wired up ultimately as the *action* that System.CommandLine will call,
	/// based on this instance. From here we *create* an instance of TModel, and populate
	/// its properties based on the ParseResult passed in, etc.
	/// </summary>
	/// <param name="parseRes">Input parse result.</param>
	/// <returns>A newly created model instance.</returns>
	public TModel GetModelInstanceAndPopulateValues(ParseResult parseRes)
	{
		//public TAuto GetBoundValue(ParseResult parseRes) //BindingContext bindingContext)
		//ParseResult parseRes = bindingContext.ParseResult;
		TModel cmdModel = new();

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
				TModel item = GetModelInstanceAndPopulateValues(r);
				Method.Invoke(item, null);
			});
		}
		else {
			Func<ParseResult, Task> action = async r => {
				TModel item = GetModelInstanceAndPopulateValues(r);
				await (Task)Method.Invoke(item, null);
			};

			Cmd.SetAction(action);
		}
	}
}

using System.CommandLine;
using System.Reflection;

namespace CommandLine.EasyBuilder.Internal;

/// <summary>
/// This type wraps a class with CommandAttribute applied to it, and herein will
/// be all its auto properties. This ends up being the glue that persists from app
/// creation, along with an instance of the CommandAttribute that was applied to the model
/// class.
/// </summary>
/// <remarks>
/// The ultimate glue that allows this type to be worked into being invoked by System.CommandLine
/// is that <see cref="Cmd"/>'s SetAction is set to call INSTANCE methods on this <see cref="CmdModelInfo"/>
/// (those instance methods create an instance of the model type, fill its properties, and
/// THEN call that model's handler, see <see cref="GetModelInstanceAndPopulateValues"/>).
/// So in short: <see cref="Command.SetAction(Action{ParseResult})"/> captures an instance
/// this class, which then is set per invocation to create the model per hit and call it's own
/// handler.
/// </remarks>
public class CmdModelInfo
{
	/// <summary>The command (view) model type</summary>
	public Type ModelType;

	public bool ModelHasConstructorWithParameters;

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

	/// <summary>
	/// Simply indicates if there is a Handle method (not null).
	/// </summary>
	/// <remarks>
	/// May seem unneeded, as caller can simply check HandleMethod != null,
	/// but the purpose is to make explicit that IT IS VALID to not have a Handle method.
	/// </remarks>
	public bool HasHandle => HandleMethod != null;

	public MethodInfo HandleMethod { get; private set; }

	public bool HandleIsAsync;

	public static Func<Type, object> InstanceGetter => BuilderDI.ModelInstanceGetter;

	public void SetHandle(MethodInfo mi, bool handleIsAsync)
	{
		HandleIsAsync = handleIsAsync;
		HandleMethod = mi;
	}

	public MethodInfo ParseResultSetter { get; set; }

	public bool HasParseResultProp => ParseResultSetter != null;

	public object GetModelInstance() => GetModelTypeInstance(ModelType, ModelHasConstructorWithParameters);

	public static object GetModelTypeInstance(Type modelType, bool hasConstructorWithParams)
	{
		var ig = InstanceGetter;

		bool normalInstance = !hasConstructorWithParams || ig == null;
		if(normalInstance)
			return Activator.CreateInstance(modelType);

		object model = ig(modelType);
		return model;
	}


	/// <summary>
	/// Creates an *instance* of the command model type, and sets it's properties
	/// based on the input ParseResult. Ultimately called every time Invoke (etc) command
	/// is triggered (called via <see cref="SetCommandActionToHandler"/> below)
	/// </summary>
	/// <param name="parseRes">Input parse result.</param>
	/// <returns>A newly created model instance.</returns>
	public object GetModelInstanceAndPopulateValues(ParseResult parseRes, out bool error)
	{
		error = false;
		object cmdModel = GetModelInstance();

		var props = Props;
		for(int i = 0; i < props.Length; i++) {
			CmdProp apg = props[i];
			bool succ = SetPropValue.TrySetValue(parseRes, apg, cmdModel, out string errorMsg);
			if(!succ) {
				error = true;
				Console.WriteLine(errorMsg ?? $"Error setting property '{apg.Prop.Name}'");
				return null;
			}
		}

		if(HasParseResultProp)
			ParseResultSetter.Invoke(cmdModel, [parseRes]);

		return cmdModel;
	}

	public void CallHandleVoid(ParseResult r)
	{
		object item = GetModelInstanceAndPopulateValues(r, out bool error);
		if(HasHandle && !error)
			HandleMethod.Invoke(item, null);
	}

	public async Task CallHandleAsync(ParseResult r)
	{
		object item = GetModelInstanceAndPopulateValues(r, out bool error);
		if(HasHandle && !error)
			await (Task)HandleMethod.Invoke(item, null);
	}

	/// <summary>
	/// Set once at initialization time, sets <see cref="Cmd"/>'s
	/// <see cref="Command.SetAction(Action{ParseResult})"/> (or etc overload) action
	/// handler to:
	/// 1) make an instance of the model as well as set its properties
	/// (via <see cref="GetModelInstanceAndPopulateValues"/>), and
	/// 2) if it has a handler, invoke it.
	/// For types without a handler, #1 still happens (create and populate the instance).
	/// I suppose this mostly would only make sense if the model's constructor does
	/// something, but that's up to it.
	/// </summary>
	/// <remarks>
	/// FUTURE?: It is possible however that we can detect
	/// if the model has no properties, in which case never instantiate it, as this is
	/// very common when a parent command has many child commands, where the parent is
	/// just a placeholder, and has not children property options or action of it's own
	/// to take... ... TBD
	/// </remarks>
	public void SetCommandActionToHandler()
	{
		if(!HandleIsAsync)
			Cmd.SetAction(CallHandleVoid);
		else
			Cmd.SetAction(CallHandleAsync);
	}

	/// <summary>
	/// Called at initialization time, finally takes this built up
	/// <see cref="CmdModelInfo"/> instance, and generates a Commmand from it,
	/// including with it's SetAction set.
	/// </summary>
	/// <returns></returns>
	public Command GetCommand()
	{
		CommandAttribute attr = CommandAttr;

		Cmd = new(name: attr.Name, description: attr.Description);

		if(attr.Alias != null)
			Cmd.Alias(attr.Alias);

		foreach(CmdProp p in Props)
			p.AddToCmd(Cmd);

		SetCommandActionToHandler();
		return Cmd;
	}

	/// <summary>
	/// Sets model and so forth onto an EXISTING Command instance (often: RootCommand
	/// is where it's needed).
	/// </summary>
	public Command SetCommand(Command cmd)
	{
		ArgumentNullException.ThrowIfNull(cmd);

		var c = Cmd = cmd;
		CommandAttribute attr = CommandAttr;

		// c.Name = attr.Name; // immutable, ... we could demand Name match, but how to handle nulle?

		// On first principled look, it would seem best to keep the originally set description if had one
		// but I've found in practise, it's easy to have a RootCommand with a generic description for
		// many scenarios... Maybe seems less principled, but otoh: we ARE intending to set the command
		// to this given instance
		// So DO overwrite Command.Description according to the model Command's Description if exists
		if(attr.Description != null) // c.Description == null && 
			c.Description = attr.Description;

		// if attr has alias: ONLY ADD if Cmd had NO aliases set (not checking per name, just ANY set)
		if(attr.Alias != null && (c.Aliases?.Count ?? 0) == 0)
			c.Alias(attr.Alias);

		foreach(CmdProp p in Props)
			p.AddToCmd(c);

		SetCommandActionToHandler();
		return Cmd;
	}
}

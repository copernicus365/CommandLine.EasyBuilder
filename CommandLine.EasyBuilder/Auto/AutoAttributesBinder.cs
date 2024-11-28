using System.CommandLine.Binding;
using System.CommandLine.Parsing;

namespace CommandLine.EasyBuilder.Auto;

public class AutoAttributesBinder<T> : BinderBase<T> where T : class, new()
{
	public readonly AutoInfo Info;

	public AutoAttributesBinder(AutoInfo autoInfo)
		=> Info = autoInfo;

	protected override T GetBoundValue(BindingContext bindingContext)
	{
		T item = new();

		ParseResult parseRes = bindingContext.ParseResult;

		var props = Info.Props;
		for(int i = 0; i < props.Length; i++)
			OptionsClassConverter.SetVal(parseRes, props[i], item);

		return item;
	}
}

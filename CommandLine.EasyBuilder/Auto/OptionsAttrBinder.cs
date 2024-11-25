using System.CommandLine;
using System.CommandLine.Binding;

using CommandLine.EasyBuilder.Private;

namespace CommandLine.EasyBuilder.Auto;

public class OptionsAttrBinder<T> : BinderBase<T> where T : class, new()
{
	public readonly OptPropGroup[] Props;

	public OptionsAttrBinder()
	{
		Props = OptionsClassConverter.GetAttributeProps<T>();
		if(Props.IsNulle())
			throw new ArgumentException("Type if invalid, no attributes / properties found");
	}

	protected override T GetBoundValue(BindingContext bindingContext)
	{
		T item = new();

		for(int i = 0; i < Props.Length; i++) {
			OptPropGroup p = Props[i];
			Option opt = p.option;
			OptionAttribute attr = p.attr;

			object value = bindingContext.ParseResult.GetValueForOption(opt);

			if(attr.DefVal != null) {
				if(value == null || value.Equals(p.defVal))
					value = attr.DefVal;
			}

			p.pi.SetValue(item, value);
		}
		return item;
	}
}

using System.CommandLine;
using System.Reflection;

using CommandLine.EasyBuilder.Private;

namespace CommandLine.EasyBuilder.Auto;

public record AutoPropGroup(Type propertyType, PropertyInfo pi, OptionAttribute attr, Option option, object defVal);

public class OptionsClassConverter
{
	static readonly Type OptAttrTyp = typeof(OptionAttribute);

	public static ControlAttribute GetControlAttr<T>()
		=> typeof(T).GetCustomAttributes(typeof(ControlAttribute), true).FirstOrDefault() as ControlAttribute;

	public static AutoPropGroup[] GetAttributeProps<T>(out ControlAttribute cntAttr) where T : class, new()
	{
		Type type = typeof(T);
		cntAttr = null;

		PropertyInfo[] props = type.GetProperties();
		if(props.IsNulle())
			return null;

		cntAttr = GetControlAttr<T>();

		AutoPropGroup[] ogroup = props
			.Select(pi => {
				if(pi.GetCustomAttribute(OptAttrTyp, true) is not OptionAttribute attr)
					return null;

				Type propTyp = pi.PropertyType;

				//Option opt = getOptT(propTyp);

				Type constructedGenType = typeof(Option<>).MakeGenericType(propTyp);
				Option opt = Activator.CreateInstance(constructedGenType, attr.Name, attr.Description) as Option;

				if(attr.Required == true)
					opt.IsRequired = true;

				//opt.Name = attr.Name;
				//opt.Description = attr.Description;
				
				if(attr.Alias.NotNulle())
					opt.AddAlias(attr.Alias);

				object defVal = null;
				if(propTyp.IsValueType)
					defVal = Activator.CreateInstance(propTyp);

				if(attr.DefVal != null && defVal != null && !attr.DefVal.Equals(defVal))
					opt.SetDefaultValue(attr.DefVal);

				AutoPropGroup vv = new(propTyp, pi, attr, opt, defVal);
				return vv;
			})
			.Where(v => v != null)
			.ToArray();

		return ogroup;
	}
}

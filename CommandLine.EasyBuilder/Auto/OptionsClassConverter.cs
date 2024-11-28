using System.CommandLine;
using System.CommandLine.Parsing;
using System.Reflection;

using CommandLine.EasyBuilder.Private;

namespace CommandLine.EasyBuilder.Auto;

public class OptionsClassConverter
{
	static readonly Type OptAttrTyp = typeof(OptionAttribute);

	public static CommandAttribute GetControlAttr<T>()
		=> typeof(T).GetCustomAttributes(typeof(CommandAttribute), true).FirstOrDefault() as CommandAttribute;

	public static object GetHandleMethod<TAuto>(AutoInfo info)
	{
		Type type = typeof(TAuto);

		MethodInfo method = type.GetMethod("Handle");
		if(method == null) {
			method = type.GetMethod("HandleAsync");
			if(method == null)
				return null;
		}

		bool isVoidRetType = method.ReturnType == typeof(void);

		object _handle = null;

		if(isVoidRetType) {
			Action<TAuto> handle = (TAuto v) => {
				info.Method.Invoke(v, null);
			};
			_handle = handle;
		}
		else {
			Func<TAuto, Task> handle = async (TAuto v) => {
				object res = info.Method.Invoke(v, null);
				Task result = res as Task;
				await result;
			};
			_handle = handle;
		}

		info.SetHandle(method, !isVoidRetType, _handle);
		return _handle;
	}

	public static AutoInfo GetAutoInfoOrThrow<T>() where T : class, new()
	{
		AutoInfo info = GetAutoInfo<T>();
		if(info == null || info.NoProperties)
			throw new ArgumentException("Type if invalid, no attributes / properties found");
		return info;
	}

	public static AutoInfo GetAutoInfo<T>() where T : class, new()
	{
		Type type = typeof(T);

		PropertyInfo[] props = type.GetProperties();
		if(props.IsNulle())
			return null;

		AutoInfo info = new() {
			TType = type,
			Command = GetControlAttr<T>(),
		};

		AutoPropGroup[] ogroup = props
			.Select(pi => {
				CommandLineValueAttribute c = pi.GetCustomAttribute(typeof(CommandLineValueAttribute), true)
				as CommandLineValueAttribute;

				OptionAttribute optattr = c as OptionAttribute;
				bool isOpt = optattr != null;
				if(!isOpt) {
					if(c is not ArgumentAttribute argAttr)
						return null;
				}

				Type propTyp = pi.PropertyType;

				Type propTypeFromNullable = !propTyp.IsValueType ? null : Nullable.GetUnderlyingType(propTyp);

				bool isNullable = propTypeFromNullable != null;
				if(isNullable)
					propTyp = propTypeFromNullable;

				Type constructedGenType = (isOpt ? typeof(Option<>) : typeof(Argument<>)).MakeGenericType(propTyp);
				Option opt = null;
				Argument arg = null;

				if(isOpt)
					opt = Activator.CreateInstance(constructedGenType, c.Name, c.Description) as Option;
				else
					arg = Activator.CreateInstance(constructedGenType, c.Name, c.Description) as Argument;

				if(isOpt) {
					if(c.Required == true)
						opt.IsRequired = true;

					if(c.Alias.NotNulle())
						opt.AddAlias(c.Alias);
				}

				// NEED to have default(T)
				// SET THESE FOR NULL, but then FIX for value types next
				object defaultT = null;
				bool hasDefPropSet = c.DefVal != null;

				if(hasDefPropSet && // if is NULL, then no use...
					propTyp.IsValueType) {

					// hasDefPropSet incorrectly said TRUE bec only tested NULL, fix for val type
					// must create an instance to compare

					defaultT = Activator.CreateInstance(propTyp);

					if(defaultT != null
						&& c.DefVal.Equals(defaultT)) {
						hasDefPropSet = false; // have OFFICIALLY stopped val types from saying they had a value set
					}
				} // no else needed, initial values handled not value type

				if(hasDefPropSet) {
					if(isOpt)
						opt.SetDefaultValue(c.DefVal);
					else
						arg.SetDefaultValue(c.DefVal);
				}

				AutoPropGroup vv = new(propTyp, isNullable, pi, c, opt, arg, defaultT);
				return vv;
			})
			.Where(v => v != null)
			.ToArray();

		info.Props = ogroup;

		return info;
	}

	/// <summary>
	/// Critical method. Used internally by AutoAttributesBinder to perform the
	/// ultimate binding and reading of types / input into custom class.
	/// 
	/// </summary>
	/// <param name="parseRes"></param>
	/// <param name="p"></param>
	/// <param name="item"></param>
	public static void SetVal(ParseResult parseRes, AutoPropGroup p, object item)
	{
		//if(Verbose) { SetValVerbose(parseRes, p, item); return; }

		object value = p.IsOption
			? parseRes.GetValueForOption(p.option)
			: parseRes.GetValueForArgument(p.argument);

		Type propTyp = p.propertyType;

		if(propTyp.IsValueType && p.isNullable)
			value = Convert.ChangeType(value, propTyp);

		bool hasDefVal = p.attr.DefVal != null;
		if(hasDefVal && (value == null || value.Equals(p.defVal)))
			value = p.attr.DefVal;

		p.pi.SetValue(item, value);
	}

	//static bool Verbose = false;
	//public static void SetValVerbose(ParseResult parseRes, AutoPropGroup p, object item)
	//{
	//	Option opt = p.option;
	//	OptionAttribute attr = p.attr;

	//	object value = parseRes.GetValueForOption(opt);

	//	Type propTyp = p.propertyType;
	//	string origTypNm = propTyp.FullName;

	//	if(propTyp.IsValueType) {
	//		if(p.isNullable) {
	//			string valTypNm1 = value.GetType().FullName;
	//			value = Convert.ChangeType(value, propTyp);
	//			string valTypNm2 = value.GetType().FullName;
	//		}
	//	}

	//	bool hasDefVal = attr.DefVal != null;

	//	if(hasDefVal) {
	//		if(value == null || value.Equals(p.defVal))
	//			value = attr.DefVal;
	//	}

	//	p.pi.SetValue(item, value);
	//}
}

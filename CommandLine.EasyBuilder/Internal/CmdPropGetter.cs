using System.CommandLine;
using System.Reflection;

namespace CommandLine.EasyBuilder.Internal;

public class CmdPropGetter(PropertyInfo pi)
{
	CLPropertyAttribute c;
	OptionAttribute optattr;

	bool isOpt;
	bool isNullable;
	bool isArray;
	bool isNumberArray;
	bool optionComesFromDirectStaticMethod;
	bool hasDefPropSet;

	Type propTyp;
	Type numberArrType;
	Type constructedGenType;

	Option opt = null;
	Argument arg = null;
	object defaultT;

	public CmdProp GetProp()
	{
		if(!_Init())
			return null;

		_SetArrayNumType();

		_SetNullableTyp();

		_MakeOptionArgumentTType();

		if(GetOptionFromFromStaticMethod(out CmdProp prop1))
			return prop1;

		_CreateOptArgInstance();

		_SetArity();

		_SetMiscOptVals();

		_SetHelpName();

		_SetDefPropSet();

		CmdProp prop = new(propTyp, isNullable, pi, c, opt, arg, c.DefVal, defaultT) {
			IsArray = isArray,
			IsNumberArray = isNumberArray,
			NumberArrayItemType = numberArrType,
		};

		return prop;
	}

	public static CmdProp PropToCmpProp(PropertyInfo pi)
		 => new CmdPropGetter(pi).GetProp();

	bool _Init()
	{
		c = pi.GetCustomAttribute(typeof(CLPropertyAttribute), true) as CLPropertyAttribute;

		optattr = c as OptionAttribute;
		isOpt = optattr != null;
		if(!isOpt) {
			if(c is not ArgumentAttribute argAttr)
				return false;
		}

		propTyp = pi.PropertyType;
		return true;
	}

	void _SetArrayNumType()
	{
		isArray = propTyp.IsArray;
		numberArrType = null;
		if(!isArray)
			return;

		numberArrType = CmdModelReflectionHelper.GetNumericArrayElementType(propTyp);
		if(numberArrType == null) {
			isArray = false;
			return;
		}

		isNumberArray = true;
		propTyp = typeof(string);
	}

	void _SetNullableTyp()
	{
		Type propTypeFromNullable = !propTyp.IsValueType ? null : Nullable.GetUnderlyingType(propTyp);

		isNullable = propTypeFromNullable != null;
	}

	void _MakeOptionArgumentTType()
		=> constructedGenType = (isOpt ? typeof(Option<>) : typeof(Argument<>)).MakeGenericType(propTyp);

	bool GetOptionFromFromStaticMethod(out CmdProp prop1)
	{
		prop1 = null;
		optionComesFromDirectStaticMethod = string.IsNullOrEmpty(c.Name);
		if(!optionComesFromDirectStaticMethod)
			return false;

		object objFromSt = CmdModelReflectionHelper.TryGetOptionFromStaticMethod(pi, isOpt, constructedGenType);
		if(objFromSt == null)
			throw new InvalidOperationException($"Property '{pi.Name}' is marked to get {(isOpt ? "Option" : "Argument")} from static method, but no valid static method found.");

		if(isOpt) {
			opt = objFromSt as Option;
			c.Name = opt.Name;
		}
		else {
			arg = objFromSt as Argument;
			c.Name = arg.Name;
		}

		prop1 = new(propTyp, isNullable, pi, c, opt, arg, c.DefVal, DefaultOfTVal: null);
		return true;
	}

	void _CreateOptArgInstance()
	{
		if(isOpt) {
			opt = Activator.CreateInstance(constructedGenType, c.Name) as Option;
			opt.Description = c.Description;
		}
		else {
			arg = Activator.CreateInstance(constructedGenType, c.Name) as Argument;
			arg.Description = c.Description;
		}
	}

	void _SetArity()
	{
		if(c.MinArity > 0 || c.MaxArity > 0) {
			int min = c.MinArity;
			int max = c.MaxArity;
			if(min < 0 || max < min)
				throw new ArgumentOutOfRangeException("MinArgs or MaxArgs values out of range");

			ArgumentArity arity = new(min, max);
			if(isOpt)
				opt.Arity = arity;
			else
				arg.Arity = arity;
		}
	}

	void _SetMiscOptVals()
	{
		if(isOpt) {
			if(c.Required == true)
				opt.Required = true;

			if(!string.IsNullOrEmpty(c.Alias))
				opt.Aliases.Add(c.Alias);

			if(c.AllowMultipleArgumentsPerToken)
				opt.AllowMultipleArgumentsPerToken = true;
		}
	}

	void _SetHelpName()
	{
		if(c.HelpName != null) {
			if(isOpt)
				opt.HelpName = c.HelpName;
			else
				arg.HelpName = c.HelpName;
		}
	}

	/// <summary>
	/// Need to clean this up some and clarify...
	/// </summary>
	void _SetDefPropSet()
	{
		// NEED to have default(T)
		// SET THESE FOR NULL, but then FIX for value types next
		defaultT = null;
		hasDefPropSet = c.DefVal != null;

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
			Delegate dlg = CmdModelReflectionHelper.GetArgResFuncReturnsObjectVal(propTyp, c.DefVal);
			PropertyInfo getValProp = constructedGenType.GetProperty("DefaultValueFactory");
			getValProp.SetValue(isOpt ? opt : arg, dlg);
		}
	}
}

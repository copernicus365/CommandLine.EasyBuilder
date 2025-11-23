using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq.Expressions;
using System.Reflection;

namespace CommandLine.EasyBuilder.Internal;

/// <summary>
/// Run once per property on command model option / argument props, once at build time only.
/// Given the huge work to discern all of this, keeping this builder (`CmdPropGetter`) its own
/// type, but it builds a `CmdProp` instance, many of which will be stored on `CmdModelInfo`.
/// </summary>
/// <param name="pi"></param>
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
	object defaultTInst;

	public CmdProp GetProp(object modelInst)
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

		_SetDefPropSet(modelInst);

		CmdProp prop = new(propTyp, isNullable, pi, c, opt, arg, c.DefVal, defaultTInst) {
			IsArray = isArray,
			IsNumberArray = isNumberArray,
			NumberArrayItemType = numberArrType,
		};

		return prop;
	}

	public static CmdProp PropToCmpProp(PropertyInfo pi, object modelInst)
		 => new CmdPropGetter(pi).GetProp(modelInst);

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

	#region --- Default Value Detection FUN ;) ---

	/// <summary>
	/// Need to clean this up some and clarify...
	/// </summary>
	void _SetDefPropSet(object modelInst)
	{
		hasDefPropSet = TryGetExplicitDefaultValue(modelInst, pi, c.DefVal, out object defValFound);
		if(!hasDefPropSet)
			return;

		c.DefVal = defValFound;

		__SetDefValueFactToDefaultValue(c.DefVal);
	}

	/// <summary>
	/// Greatly revamped! Far better than prior... heavy lifting is here in discerning
	/// both if c.DefVal was set, and if not, if the property has an auto-property set (!).
	/// </summary>
	/// <param name="parentObj"></param>
	/// <param name="prop"></param>
	/// <param name="inputDefVal">
	/// EVEN if inputDefVal was a value type, we assume it EVER getting set, EVEN as default value (eg '0' for int)
	/// means to use it. (Eg DefVal = 0). Because `DefVal` is an `object` from teh beginning, so it should always be NULL
	/// if ti wasn't explicitly set. So let this take precedence if not null: KISS!
	/// Another note: We only read / use parentObj if inputDefVal was null.
	/// </param>
	/// <param name="value"></param>
	public static bool TryGetExplicitDefaultValue(object parentObj, PropertyInfo prop, object inputDefVal, out object value)
	{
		value = null;
		if(prop == null || !prop.CanRead)
			return false;

		object val = inputDefVal;
		if(val == null) {
			if(parentObj == null)
				return false;

			val = prop.GetValue(obj: parentObj); // ok, get value from the instance class
		}

		if(!prop.PropertyType.IsValueType) {
			if(val == null)
				return false;
			value = val; // val is NOT null, and value IS a ref type, RETURN TRUE
			return true;
		}

		if(val == null) // covers IsNullable issues
			return false;

		// BELOW: VALUE TYPES ONLY, and val is NOT null

		object propertyTypeDefValue = Activator.CreateInstance(prop.PropertyType); // if T is int, typeDefValue == `0` (boxed)
		if(propertyTypeDefValue == null || val.Equals(propertyTypeDefValue))
			return false;

		value = val;
		return true;
	}

	/// <summary>
	/// How SysCL works: sets this heavy `DefaultValueFactory`, so goal here is,
	/// to get the Option or Argument (here either `opt` or `arg` objects we've already instantiated),
	/// get it's `DefaultValueFactory` property, BUT!! that's only a member on the generic version,
	/// g `Option{T}`... So we FIRST have to use some heavy reflection, using the `constructedGenType`
	/// we already got when we made `opt` or `arg` above, ie the typed T version...
	/// Even when we get the `PropertyInfo valueFactoryProp`, to actually set its value is ... yeah,
	/// ugly, but seems to be working! 
	/// </summary>
	void __SetDefValueFactToDefaultValue(object defVal)
	{
		// #1: get the 'DefaultValueFactory' property on constructedGenType
		Delegate dlg = GetArgResFuncReturnsObjectVal(propTyp, defVal);
		PropertyInfo valueFactoryProp = constructedGenType.GetProperty("DefaultValueFactory");
		valueFactoryProp.SetValue(isOpt ? opt : arg, dlg);
	}

	public static Delegate GetArgResFuncReturnsObjectVal(Type genericPropTyp, object val)
	{
		// thank you, GROK
		// Build Func<ArgumentResult, T> (ArgumentResult arg) => (T)val (as a constant expression, ignoring the input)
		Type funcType = typeof(Func<,>).MakeGenericType(typeof(ArgumentResult), genericPropTyp);
		ParameterExpression param = Expression.Parameter(typeof(ArgumentResult)); //, "arg");
		Expression body = Expression.Constant(val, genericPropTyp);
		LambdaExpression lambdaExpr = Expression.Lambda(funcType, body, param);
		Delegate constantFunc = lambdaExpr.Compile();
		return constantFunc;
	}

	#endregion

}

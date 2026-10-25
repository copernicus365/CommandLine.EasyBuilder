using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace CommandLine.EasyBuilder.Internal;

/// <summary>
/// Run once per property on command model option / argument props, once at build time only.
/// Given the huge work to discern all of this, keeping this builder (`CmdPropGetter`) its own
/// type, but it builds a `CmdProp` instance, many of which will be stored on `CmdModelInfo`.
/// </summary>
public class CmdPropGetter
{
	[DynamicallyAccessedMembers(CmdModelReflectionHelper.ModelMembers)]
	readonly Type modelType;
	readonly PropertyInfo pi;

	CLPropertyAttribute c;
	OptionAttribute optattr;

	bool isOpt;
	bool isNullable;
	bool isArray;
	bool isNumberArray;
	bool isNoValueFlag;
	bool optionComesFromDirectStaticMethod;
	bool hasDefPropSet;

	Type propTyp;
	Type numberArrType;
#if EASYBUILDER_LEGACY
	Type constructedGenType;
#else
	IPropTypeBinder binder;
#endif

	Option opt = null;
	Argument arg = null;

	/// <summary>Gets the option or argument for property <paramref name="pi"/> of a command (view) model.</summary>
	/// <param name="modelType">The command (view) model type, where a static <c>Get{PropName}Option()</c> is looked for, its base classes included.</param>
	/// <param name="pi">The property.</param>
	public CmdPropGetter([DynamicallyAccessedMembers(CmdModelReflectionHelper.ModelMembers)] Type modelType, PropertyInfo pi)
	{
		this.modelType = modelType;
		this.pi = pi;
	}

	/// <summary>Gets the option or argument for property <paramref name="pi"/>, looking for a static <c>Get{PropName}Option()</c> from its declaring type.</summary>
	/// <param name="pi">The property.</param>
	[RequiresUnreferencedCode("Looks for a static Get{PropName}Option() from the property's declaring type, which trimming can't follow. Use the constructor that takes the command class's type.")]
	public CmdPropGetter(PropertyInfo pi)
		: this(pi.DeclaringType, pi) { }

	public CmdProp GetProp(object modelInst)
	{
		if(!_Init())
			return null;

		_SetArrayNumType();

		_SetNullableTyp();

#if EASYBUILDER_LEGACY
		_MakeOptionArgumentTType();
#else
		_SetBinder();
#endif

		if(GetOptionFromFromStaticMethod(out CmdProp prop1))
			return prop1;

		_CreateOptArgInstance();

		_SetArity();

		_SetMiscOptVals();

		_SetHelpName();

		_SetDefPropSet(modelInst);

		_CheckNoValueFlagDefault();

		CmdProp prop = new(propTyp, isNullable, pi, c, opt, arg, c.DefVal, DefaultOfTVal: null) {
			IsArray = isArray,
			IsNumberArray = isNumberArray,
			NumberArrayItemType = numberArrType,
#if !EASYBUILDER_LEGACY
			Binder = binder,
#endif
		};

		return prop;
	}

	public static CmdProp PropToCmpProp([DynamicallyAccessedMembers(CmdModelReflectionHelper.ModelMembers)] Type modelType, PropertyInfo pi, object modelInst)
		=> new CmdPropGetter(modelType, pi).GetProp(modelInst);

	[RequiresUnreferencedCode("Looks for a static Get{PropName}Option() from the property's declaring type, which trimming can't follow. Use the overload that takes the command class's type.")]
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

#if EASYBUILDER_LEGACY
	void _MakeOptionArgumentTType()
		=> constructedGenType = (isOpt ? typeof(Option<>) : typeof(Argument<>)).MakeGenericType(propTyp);
#else
	/// <summary>The property type's binder, or the property refused: in a native AOT build, a type neither built in nor registered.</summary>
	void _SetBinder()
	{
		binder = PropTypeBinders.GetBinder(propTyp);
		if(binder == null)
			throw new InvalidOperationException(PropTypeBinders.NoBinderMessage(pi, propTyp));
	}
#endif

	bool GetOptionFromFromStaticMethod(out CmdProp prop1)
	{
		prop1 = null;
		optionComesFromDirectStaticMethod = string.IsNullOrEmpty(c.Name);
		if(!optionComesFromDirectStaticMethod)
			return false;

#if EASYBUILDER_LEGACY
		// pre-AOT way, kept until the new lookup has run in the wild: statics of the property's declaring class only
		object objFromSt = CmdModelReflectionHelper.TryGetOptionFromStaticMethod(pi, isOpt, constructedGenType);
#else
		// a number array binds as an Option<string>, which a static's Option<T> of the property's type never is
		if(propTyp != pi.PropertyType)
			throw new ArgumentException($"Get static option or argument type is invalid for property '{pi.Name}' ({pi.PropertyType.FullName})");

		object objFromSt = CmdModelReflectionHelper.TryGetOptionFromStaticMethod(modelType, pi, isOpt);
#endif
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

		prop1 = new(propTyp, isNullable, pi, c, opt, arg, c.DefVal, DefaultOfTVal: null) {
#if !EASYBUILDER_LEGACY
			Binder = binder,
#endif
		};
		return true;
	}

	void _CreateOptArgInstance()
	{
#if EASYBUILDER_LEGACY
		// pre-AOT way, kept until the typed binders have run in the wild: the Option<T> / Argument<T> made by reflection
		if(isOpt) {
			opt = Activator.CreateInstance(constructedGenType, c.Name) as Option;
			opt.Description = c.Description;
		}
		else {
			arg = Activator.CreateInstance(constructedGenType, c.Name) as Argument;
			arg.Description = c.Description;
		}
#else
		if(isOpt) {
			opt = binder.CreateOption(c.Name);
			opt.Description = c.Description;
		}
		else {
			arg = binder.CreateArgument(c.Name);
			arg.Description = c.Description;
		}
#endif
	}

	void _SetArity()
	{
		int min = c.MinArity;
		int max = c.MaxArity;

		// -1 is not set: neither set leaves System.CommandLine's own arity for the type
		if(min == -1 && max == -1)
			return;

		// one set, and the other unset reads as 0
		if(min == -1)
			min = 0;

		if(max == -1)
			max = 0;

		if(min < 0 || max < min)
			throw new ArgumentOutOfRangeException("MinArgs or MaxArgs values out of range");

		// a zero-arity string option reads null, an int 0, given or not: only a bool flag means something
		isNoValueFlag = max == 0;

		Type declaredTyp = pi.PropertyType;
		if(isNoValueFlag && declaredTyp != typeof(bool) && declaredTyp != typeof(bool?)) {
			string msg = $"Property '{pi.Name}' takes no value (MaxArity = 0), which only a bool or bool? property can; it is a {PropTypeBinders.TypeName(declaredTyp)}.";
			throw new InvalidOperationException(msg);
		}

		ArgumentArity arity = new(min, max);
		if(isOpt)
			opt.Arity = arity;
		else
			arg.Arity = arity;
	}

	/// <summary>A flag that takes no value can never be false if its default is true, so that's refused.</summary>
	void _CheckNoValueFlagDefault()
	{
		if(!isNoValueFlag || c.DefVal is not true)
			return;

		string msg = $"Property '{pi.Name}' takes no value (MaxArity = 0), so its default can't be true: it could never be false.";
		throw new InvalidOperationException(msg);
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
#if EASYBUILDER_LEGACY
		// pre-AOT way, kept until the typed binders have run in the wild: the type's default made by Activator.CreateInstance
		hasDefPropSet = TryGetExplicitDefaultValue(modelInst, pi, c.DefVal, out object defValFound);
#else
		hasDefPropSet = TryGetExplicitDefaultValue(modelInst, pi, c.DefVal, binder.DefaultOfT, out object defValFound);
#endif
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
	/// <param name="propertyTypeDefValue">
	/// The property type's default value, boxed (if T is int, `0`): a value type's value equal to it isn't a default.
	/// </param>
	/// <param name="value"></param>
	public static bool TryGetExplicitDefaultValue(object parentObj, PropertyInfo prop, object inputDefVal, object propertyTypeDefValue, out object value)
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

		if(propertyTypeDefValue == null || val.Equals(propertyTypeDefValue))
			return false;

		value = val;
		return true;
	}

	/// <summary>
	/// As <see cref="TryGetExplicitDefaultValue(object, PropertyInfo, object, object, out object)"/>, with the
	/// property type's default made by <c>Activator.CreateInstance</c>.
	/// </summary>
	[RequiresUnreferencedCode("Makes the property type's default with Activator.CreateInstance, which trimming can't follow. Use the overload that takes that default.")]
	public static bool TryGetExplicitDefaultValue(object parentObj, PropertyInfo prop, object inputDefVal, out object value)
	{
#if EASYBUILDER_LEGACY
		// pre-AOT way, kept until the typed binders have run in the wild: the type's default made by Activator.CreateInstance
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
#else
		// if T is int, `0` (boxed); only a value type's is ever compared
		object propertyTypeDefValue = prop != null && prop.PropertyType.IsValueType
			? Activator.CreateInstance(prop.PropertyType)
			: null;

		return TryGetExplicitDefaultValue(parentObj, prop, inputDefVal, propertyTypeDefValue, out value);
#endif
	}

	/// <summary>
	/// How SysCL works: sets this heavy `DefaultValueFactory`, so goal here is,
	/// to get the Option or Argument (here either `opt` or `arg` objects we've already instantiated),
	/// get it's `DefaultValueFactory` property, BUT!! that's only a member on the generic version,
	/// g `Option{T}`... So we FIRST have to use some heavy reflection, using the `constructedGenType`
	/// we already got when we made `opt` or `arg` above, ie the typed T version...
	/// Even when we get the `PropertyInfo valueFactoryProp`, to actually set its value is ... yeah,
	/// ugly, but seems to be working!
	/// Outside EASYBUILDER_LEGACY, the property type's binder sets it, typed.
	/// </summary>
	void __SetDefValueFactToDefaultValue(object defVal)
	{
#if EASYBUILDER_LEGACY
		// pre-AOT way, kept until the typed binders have run in the wild: DefaultValueFactory found and set by reflection
		// #1: get the 'DefaultValueFactory' property on constructedGenType
		Delegate dlg = GetArgResFuncReturnsObjectVal(propTyp, defVal);
		PropertyInfo valueFactoryProp = constructedGenType.GetProperty("DefaultValueFactory");
		valueFactoryProp.SetValue(isOpt ? opt : arg, dlg);
#else
		// IsInstanceOfType is the test Expression.Constant makes in the legacy path: a cast alone would also
		// take an integer as an enum's default, or an enum as an integer's
		if(!propTyp.IsInstanceOfType(defVal)) {
			string msg = $"Property '{pi.Name}' has the default {defVal} ({PropTypeBinders.TypeName(defVal.GetType())}), which can't be the default of its {(isOpt ? "option" : "argument")}, of type {PropTypeBinders.TypeName(propTyp)}.";
			throw new InvalidCastException(msg);
		}

		binder.SetDefault(isOpt ? opt : arg, defVal);
#endif
	}

	[RequiresDynamicCode("Builds the default's delegate with MakeGenericType and a compiled expression, which a native AOT build can't run. EasyBuilder itself sets defaults through IPropTypeBinder.SetDefault.")]
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

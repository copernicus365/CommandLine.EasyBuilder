using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace GuardProbe;

// Probe: which of the plan's patterns the trim/AOT analyzer accepts, on net8.0 and net10.0.
// Each pattern sits in its own method so a warning's line number names the pattern.

public interface IPropBinder
{
	Option CreateOption(string name);
	object GetValue(ParseResult pr, Option opt);
	void SetDefault(Option opt, object value);
	object DefaultOfT { get; }
}

public sealed class PropBinder<T> : IPropBinder
{
	public Option CreateOption(string name) => new Option<T>(name);

	public object GetValue(ParseResult pr, Option opt) => pr.GetValue((Option<T>)opt);

	public void SetDefault(Option opt, object value)
	{
		T typed = (T)value;
		((Option<T>)opt).DefaultValueFactory = _ => typed;
	}

	public object DefaultOfT => default(T);
}

public sealed class Holder
{
	[DynamicallyAccessedMembers(Probe.ModelMembers)]
	public Type ModelType;
}

public static class Probe
{
	public const DynamicallyAccessedMemberTypes ModelMembers =
		DynamicallyAccessedMemberTypes.PublicConstructors
		| DynamicallyAccessedMemberTypes.PublicProperties
		| DynamicallyAccessedMemberTypes.PublicMethods;

	static readonly Dictionary<Type, IPropBinder> _registered = [];

	// P1: MakeGenericMethod on a statically known method, guarded by an early return
	public static IPropBinder P1_FallbackGuardedEarlyReturn(Type t)
	{
		if(!RuntimeFeature.IsDynamicCodeSupported)
			return null;

		MethodInfo mi = typeof(Probe).GetMethod(nameof(CreateBinder), BindingFlags.NonPublic | BindingFlags.Static);
		return (IPropBinder)mi.MakeGenericMethod(t).Invoke(null, null);
	}

	// P2: same, guarded by an if-block
	public static IPropBinder P2_FallbackGuardedBlock(Type t)
	{
		if(RuntimeFeature.IsDynamicCodeSupported) {
			MethodInfo mi = typeof(Probe).GetMethod(nameof(CreateBinder), BindingFlags.NonPublic | BindingFlags.Static);
			return (IPropBinder)mi.MakeGenericMethod(t).Invoke(null, null);
		}
		return null;
	}

	// P3: control, unguarded
	public static IPropBinder P3_FallbackUnguarded(Type t)
	{
		MethodInfo mi = typeof(Probe).GetMethod(nameof(CreateBinder), BindingFlags.NonPublic | BindingFlags.Static);
		return (IPropBinder)mi.MakeGenericMethod(t).Invoke(null, null);
	}

	static IPropBinder CreateBinder<T>() => new PropBinder<T>();

	// P4: control, Array.CreateInstance
	public static Array P4_ArrayCreateInstance(Type elementType, int n)
		=> Array.CreateInstance(elementType, n);

#if NET9_0_OR_GREATER
	// P5: Array.CreateInstanceFromArrayType (net9+)
	public static Array P5_ArrayFromArrayType(Type arrayType, int n)
		=> Array.CreateInstanceFromArrayType(arrayType, n);
#endif

	// P6: enum by Type
	public static object P6_ParseEnum(Type enumType, string s)
		=> Enum.TryParse(enumType, s, ignoreCase: true, out object v) ? v : null;

	public static string[] P6_EnumNames(Type enumType)
		=> Enum.GetNames(enumType);

	// P7: the model type's flow, annotated
	public static object P7_ModelGeneric<[DynamicallyAccessedMembers(ModelMembers)] TModel>()
		=> P7_Model(typeof(TModel));

	public static object P7_Model([DynamicallyAccessedMembers(ModelMembers)] Type modelType)
	{
		PropertyInfo[] props = modelType.GetProperties();
		MethodInfo handle = modelType.GetMethod("Handle");
		MethodInfo[] statics = modelType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
		bool ctorParams = modelType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Any(c => c.GetParameters().Length > 0);

		Holder h = new() { ModelType = modelType };
		object inst = Activator.CreateInstance(h.ModelType);

		return (props, handle, statics, ctorParams, inst);
	}

	// P8: the static-method path's type check, without MakeGenericType
	public static bool P8_ReturnsOptionOf(MethodInfo m, Type t)
		=> m.ReturnType.IsGenericType
		&& m.ReturnType.GetGenericTypeDefinition() == typeof(Option<>)
		&& m.ReturnType.GetGenericArguments()[0] == t;

	// P9: registration from app code
	public static void P9_Register<T>()
		=> _registered[typeof(T)] = new PropBinder<T>();

	// P10: control, the static-method lookup on pi.DeclaringType as today
	public static MethodInfo[] P10_DeclaringTypeStatics(PropertyInfo pi)
		=> pi.DeclaringType.GetMethods(BindingFlags.Static | BindingFlags.Public);
}

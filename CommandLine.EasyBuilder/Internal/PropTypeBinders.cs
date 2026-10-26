using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CommandLine.EasyBuilder.Internal;

/// <summary>
/// Finds the <see cref="IPropTypeBinder"/> for a property's type, once per type when the command tree is
/// built, then caches it. In order: (1) the built-in table, every type System.CommandLine parses on its own;
/// (2) the types an app registered through <see cref="AutoBuilder.RegisterTypes"/>; (3) only while dynamic
/// code is on (never in a native AOT build), a binder made by reflection, for any other type. Otherwise there
/// is none, and the property is refused, naming the registration step to add.
/// </summary>
public static class PropTypeBinders
{
	// registered and resolved binders share one cache: whichever step supplied a type's binder, it's PropTypeBinder<T>
	static readonly ConcurrentDictionary<Type, IPropTypeBinder> binders = new();

	/// <summary>Registers the binder for exactly <typeparamref name="T"/>.</summary>
	public static void Register<T>()
		=> binders[typeof(T)] = new PropTypeBinder<T>();

	/// <summary>
	/// The binder for <paramref name="type"/>, or null if there's none: a type neither built in nor registered,
	/// while dynamic code is off.
	/// </summary>
	public static IPropTypeBinder GetBinder(Type type)
	{
		if(binders.TryGetValue(type, out IPropTypeBinder binder))
			return binder;

		binder = BuiltIn(type) ?? MakeByReflection(type);
		if(binder != null)
			binders[type] = binder;

		return binder;
	}

	/// <summary>
	/// The built-in table: every type System.CommandLine parses on its own, each with its nullable form and its
	/// array. Not int[], long[], double[] or decimal[]: EasyBuilder binds those itself, as a comma list in an
	/// Option&lt;string&gt;, so a property of one of them never asks for its own binder.
	/// </summary>
	static IPropTypeBinder BuiltIn(Type type) => type switch {
		Type t when t == typeof(bool) => new PropTypeBinder<bool>(),
		Type t when t == typeof(bool?) => new PropTypeBinder<bool?>(),
		Type t when t == typeof(bool[]) => new PropTypeBinder<bool[]>(),

		Type t when t == typeof(byte) => new PropTypeBinder<byte>(),
		Type t when t == typeof(byte?) => new PropTypeBinder<byte?>(),
		Type t when t == typeof(byte[]) => new PropTypeBinder<byte[]>(),

		Type t when t == typeof(sbyte) => new PropTypeBinder<sbyte>(),
		Type t when t == typeof(sbyte?) => new PropTypeBinder<sbyte?>(),
		Type t when t == typeof(sbyte[]) => new PropTypeBinder<sbyte[]>(),

		Type t when t == typeof(short) => new PropTypeBinder<short>(),
		Type t when t == typeof(short?) => new PropTypeBinder<short?>(),
		Type t when t == typeof(short[]) => new PropTypeBinder<short[]>(),

		Type t when t == typeof(ushort) => new PropTypeBinder<ushort>(),
		Type t when t == typeof(ushort?) => new PropTypeBinder<ushort?>(),
		Type t when t == typeof(ushort[]) => new PropTypeBinder<ushort[]>(),

		Type t when t == typeof(int) => new PropTypeBinder<int>(),
		Type t when t == typeof(int?) => new PropTypeBinder<int?>(),

		Type t when t == typeof(uint) => new PropTypeBinder<uint>(),
		Type t when t == typeof(uint?) => new PropTypeBinder<uint?>(),
		Type t when t == typeof(uint[]) => new PropTypeBinder<uint[]>(),

		Type t when t == typeof(long) => new PropTypeBinder<long>(),
		Type t when t == typeof(long?) => new PropTypeBinder<long?>(),

		Type t when t == typeof(ulong) => new PropTypeBinder<ulong>(),
		Type t when t == typeof(ulong?) => new PropTypeBinder<ulong?>(),
		Type t when t == typeof(ulong[]) => new PropTypeBinder<ulong[]>(),

		Type t when t == typeof(float) => new PropTypeBinder<float>(),
		Type t when t == typeof(float?) => new PropTypeBinder<float?>(),
		Type t when t == typeof(float[]) => new PropTypeBinder<float[]>(),

		Type t when t == typeof(double) => new PropTypeBinder<double>(),
		Type t when t == typeof(double?) => new PropTypeBinder<double?>(),

		Type t when t == typeof(decimal) => new PropTypeBinder<decimal>(),
		Type t when t == typeof(decimal?) => new PropTypeBinder<decimal?>(),

		Type t when t == typeof(DateTime) => new PropTypeBinder<DateTime>(),
		Type t when t == typeof(DateTime?) => new PropTypeBinder<DateTime?>(),
		Type t when t == typeof(DateTime[]) => new PropTypeBinder<DateTime[]>(),

		Type t when t == typeof(DateTimeOffset) => new PropTypeBinder<DateTimeOffset>(),
		Type t when t == typeof(DateTimeOffset?) => new PropTypeBinder<DateTimeOffset?>(),
		Type t when t == typeof(DateTimeOffset[]) => new PropTypeBinder<DateTimeOffset[]>(),

		Type t when t == typeof(DateOnly) => new PropTypeBinder<DateOnly>(),
		Type t when t == typeof(DateOnly?) => new PropTypeBinder<DateOnly?>(),
		Type t when t == typeof(DateOnly[]) => new PropTypeBinder<DateOnly[]>(),

		Type t when t == typeof(TimeOnly) => new PropTypeBinder<TimeOnly>(),
		Type t when t == typeof(TimeOnly?) => new PropTypeBinder<TimeOnly?>(),
		Type t when t == typeof(TimeOnly[]) => new PropTypeBinder<TimeOnly[]>(),

		Type t when t == typeof(TimeSpan) => new PropTypeBinder<TimeSpan>(),
		Type t when t == typeof(TimeSpan?) => new PropTypeBinder<TimeSpan?>(),
		Type t when t == typeof(TimeSpan[]) => new PropTypeBinder<TimeSpan[]>(),

		Type t when t == typeof(Guid) => new PropTypeBinder<Guid>(),
		Type t when t == typeof(Guid?) => new PropTypeBinder<Guid?>(),
		Type t when t == typeof(Guid[]) => new PropTypeBinder<Guid[]>(),

		Type t when t == typeof(string) => new PropTypeBinder<string>(),
		Type t when t == typeof(string[]) => new PropTypeBinder<string[]>(),

		Type t when t == typeof(FileInfo) => new PropTypeBinder<FileInfo>(),
		Type t when t == typeof(FileInfo[]) => new PropTypeBinder<FileInfo[]>(),

		Type t when t == typeof(DirectoryInfo) => new PropTypeBinder<DirectoryInfo>(),
		Type t when t == typeof(DirectoryInfo[]) => new PropTypeBinder<DirectoryInfo[]>(),

		Type t when t == typeof(FileSystemInfo) => new PropTypeBinder<FileSystemInfo>(),
		Type t when t == typeof(FileSystemInfo[]) => new PropTypeBinder<FileSystemInfo[]>(),

		_ => null,
	};

	/// <summary>
	/// Any other type, while dynamic code is on: its binder made by reflection, so an app that isn't native AOT
	/// binds any type System.CommandLine can, as EasyBuilder always has. A native AOT build has dynamic code off,
	/// and compiles this away.
	/// </summary>
	static IPropTypeBinder MakeByReflection(Type type)
	{
		if(!RuntimeFeature.IsDynamicCodeSupported)
			return null;

		MethodInfo create = typeof(PropTypeBinders).GetMethod(nameof(CreateBinder), BindingFlags.NonPublic | BindingFlags.Static);
		return (IPropTypeBinder)create.MakeGenericMethod(type).Invoke(null, null);
	}

	static IPropTypeBinder CreateBinder<T>() => new PropTypeBinder<T>();

	/// <summary>The refusal for property <paramref name="pi"/>, whose type has no binder, naming the registration step to add.</summary>
	internal static string NoBinderMessage(PropertyInfo pi, Type type)
		=> $"Property '{pi.Name}' of type '{TypeName(type)}' can't be bound with dynamic code off (a native AOT build, or `dotnet run` of an app with PublishAot on, as a file-based app has by default). Add `AutoBuilder.RegisterTypes().{RegistrationStep(type)};` before the command tree is built.";

	/// <summary>
	/// The <see cref="TypeRegistrar"/> step that registers <paramref name="type"/>: <c>AddEnum&lt;Color&gt;()</c> for
	/// any form of an enum (Color, Color? or Color[]), otherwise <c>Add&lt;T&gt;()</c> for exactly the type.
	/// </summary>
	static string RegistrationStep(Type type)
	{
		Type enumType = EnumOf(type);

		return enumType != null
			? $"AddEnum<{TypeName(enumType)}>()"
			: $"Add<{TypeName(type)}>()";
	}

	/// <summary>The enum <paramref name="type"/> is a form of (the enum itself, its nullable form or its array), or null.</summary>
	static Type EnumOf(Type type)
	{
		Type core = Nullable.GetUnderlyingType(type) ?? (type.IsArray ? type.GetElementType() : type);
		return core.IsEnum ? core : null;
	}

	/// <summary>A type's name as C# writes it: Point, Color?, Point[], List&lt;Point&gt;, Outer.Inner.</summary>
	internal static string TypeName(Type type)
	{
		if(Nullable.GetUnderlyingType(type) is Type underlying)
			return TypeName(underlying) + "?";

		if(type.IsArray)
			return TypeName(type.GetElementType()) + "[" + new string(',', type.GetArrayRank() - 1) + "]";

		string name = type.Name;

		if(type.IsConstructedGenericType) {
			int tick = name.IndexOf('`');
			string typeArgs = string.Join(", ", type.GenericTypeArguments.Select(TypeName));
			name = $"{(tick < 0 ? name : name[..tick])}<{typeArgs}>";
		}

		return type.IsNested && type.DeclaringType != null
			? TypeName(type.DeclaringType) + "." + name
			: name;
	}
}

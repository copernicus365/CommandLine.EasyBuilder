using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

using GuardProbe;

using static System.Console;

// Probe: do the plan's routes run under native AOT, not only pass the analyzer?

WriteLine($"IsDynamicCodeSupported = {RuntimeFeature.IsDynamicCodeSupported}");

Model model = new();
PropertyInfo[] props = Props<Model>();
PropertyInfo P(string name) => props.Single(p => p.Name == name);

Section("1. Enum by Type (string route)");
Try("names of Color", () => string.Join("|", Enum.GetNames(P("C").PropertyType)));
Try("Color <- \"GREEN\"", () => {
	P("C").SetValue(model, Probe.P6_ParseEnum(P("C").PropertyType, "GREEN"));
	return model.C;
});
Try("Color? <- \"blue\"", () => {
	Type t = Nullable.GetUnderlyingType(P("Cn").PropertyType);
	P("Cn").SetValue(model, Probe.P6_ParseEnum(t, "blue"));
	return model.Cn;
});
Try("Perms <- \"read,write\"", () => {
	P("Pm").SetValue(model, Probe.P6_ParseEnum(P("Pm").PropertyType, "read,write"));
	return model.Pm;
});
Try("Color <- \"2\" (a number)", () => {
	P("C").SetValue(model, Probe.P6_ParseEnum(P("C").PropertyType, "2"));
	return model.C;
});
Try("Color <- \"99\" (a number no name has)", () => {
	P("C").SetValue(model, Probe.P6_ParseEnum(P("C").PropertyType, "99"));
	return $"{model.C} ({(int)model.C})";
});
Try("Perms <- \"3\" (a number, [Flags])", () => {
	P("Pm").SetValue(model, Probe.P6_ParseEnum(P("Pm").PropertyType, "3"));
	return model.Pm;
});
Try("Color? <- \"1\" (a number, nullable)", () => {
	Type t = Nullable.GetUnderlyingType(P("Cn").PropertyType);
	P("Cn").SetValue(model, Probe.P6_ParseEnum(t, "1"));
	return model.Cn;
});
Try("Color[] <- [\"2\", \"red\"] (a number and a name)", () => {
	Type arrType = P("Cs").PropertyType;
	Array arr = Probe.P5_ArrayFromArrayType(arrType, 2);
	arr.SetValue(Probe.P6_ParseEnum(arrType.GetElementType(), "2"), 0);
	arr.SetValue(Probe.P6_ParseEnum(arrType.GetElementType(), "red"), 1);
	P("Cs").SetValue(model, arr);
	return $"[{string.Join(", ", model.Cs)}]";
});

Section("2. Enum array: typed array built from the property's Type");
Try("CreateInstanceFromArrayType(Color[])", () => {
	Type arrType = P("Cs").PropertyType;
	Array arr = Probe.P5_ArrayFromArrayType(arrType, 2);
	arr.SetValue(Probe.P6_ParseEnum(arrType.GetElementType(), "red"), 0);
	arr.SetValue(Probe.P6_ParseEnum(arrType.GetElementType(), "blue"), 1);
	P("Cs").SetValue(model, arr);
	return $"{model.Cs.GetType().Name} [{string.Join(", ", model.Cs)}] linq:{string.Join("+", model.Cs.Select(c => c.ToString()))} hasBlue:{model.Cs.Contains(Color.Blue)}";
});
Try("CreateInstanceFromArrayType(Shade2[]), element type seen nowhere else", () => {
	Type arrType = P("Ss").PropertyType;
	Array arr = Probe.P5_ArrayFromArrayType(arrType, 1);
	arr.SetValue(Probe.P6_ParseEnum(arrType.GetElementType(), "dark"), 0);
	P("Ss").SetValue(model, arr);
	return $"{model.Ss.GetType().Name} [{string.Join(", ", model.Ss)}]";
});
Try("Array.CreateInstance(Color2, n), element type seen nowhere else", () => {
	Type elem = P("C2s").PropertyType.GetElementType();
	Array arr = Probe.P4_ArrayCreateInstance(elem, 1);
	arr.SetValue(Probe.P6_ParseEnum(elem, "teal"), 0);
	P("C2s").SetValue(model, arr);
	return $"{model.C2s.GetType().Name} [{string.Join(", ", model.C2s)}]";
});

Section("3. Typed binders (table entry, registration) through System.CommandLine");
Try("PropBinder<bool>, Arity Zero, \"--flag\"", () => Bind(new PropBinder<bool>(), "--flag", zeroArity: true, defVal: null, cmdln: "--flag"));
Try("PropBinder<bool>, Arity Zero, \"--flag false\"", () => Bind(new PropBinder<bool>(), "--flag", zeroArity: true, defVal: null, cmdln: "--flag false"));
Try("PropBinder<int>, default 5, no input", () => Bind(new PropBinder<int>(), "--n", zeroArity: false, defVal: 5, cmdln: ""));
Try("PropBinder<double?>, default 42.0, no input", () => Bind(new PropBinder<double?>(), "--d", zeroArity: false, defVal: 42.0, cmdln: ""));
Register<Shade>();
Try("Register<Shade>(), \"--shade dark\"", () => Bind(_reg[typeof(Shade)], "--shade", zeroArity: false, defVal: null, cmdln: "--shade dark"));
Try("Register<Shade>(), Shade[] \"--shades light --shades dark\"", () => Bind(_reg[typeof(Shade[])], "--shades", zeroArity: false, defVal: null, cmdln: "--shades light --shades dark"));

Section("4. Guarded JIT fallback in a native build");
Try("fallback for Point2", () => Probe.P1_FallbackGuardedEarlyReturn(typeof(Point2))?.ToString() ?? "null (branch compiled away)");

Section("5. Help for an enum bound as Option<string> + names");
{
	RootCommand root = new("probe");
	Option<string> colorOpt = new("--color") { Description = "Option<string> + Color names" };
	colorOpt.CompletionSources.Add(Enum.GetNames(P("C").PropertyType));
	Option<string> colorD = new("--color-d") { Description = "same, default Green", DefaultValueFactory = _ => "Green" };
	colorD.CompletionSources.Add(Enum.GetNames(P("C").PropertyType));
	root.Options.Add(colorOpt);
	root.Options.Add(colorD);
	root.Parse("-h").Invoke();
}

// ---------------------------------------------------------------------

static PropertyInfo[] Props<[DynamicallyAccessedMembers(Probe.ModelMembers)] T>()
	=> typeof(T).GetProperties();

static string Bind(IPropBinder b, string name, bool zeroArity, object defVal, string cmdln)
{
	RootCommand root = new("probe");
	Option opt = b.CreateOption(name);

	if(zeroArity)
		opt.Arity = ArgumentArity.Zero;

	if(defVal != null)
		b.SetDefault(opt, defVal);

	root.Options.Add(opt);

	ParseResult r = root.Parse(cmdln);
	if(r.Errors.Count > 0)
		return "ERROR: " + string.Join(" / ", r.Errors.Select(e => e.Message));

	object v = b.GetValue(r, opt);
	return v is Array a ? $"{a.GetType().Name} [{string.Join(", ", a.Cast<object>())}]" : $"{v ?? "null"} ({opt.GetType().Name})";
}

static void Register<T>()
{
	_reg[typeof(T)] = new PropBinder<T>();
	_reg[typeof(T[])] = new PropBinder<T[]>();
}

static void Section(string title)
{
	WriteLine();
	WriteLine($"===== {title} =====");
}

static void Try(string label, Func<object> f)
{
	try {
		WriteLine($"{label} -> {f()}");
	}
	catch(Exception ex) {
		WriteLine($"{label} -> THREW {ex.GetType().Name}: {ex.Message}");
	}
}

partial class Program
{
	static readonly Dictionary<Type, IPropBinder> _reg = [];
}

public class Model
{
	public Color C { get; set; }
	public Color? Cn { get; set; }
	public Color[] Cs { get; set; }
	public Perms Pm { get; set; }
	public Shade2[] Ss { get; set; }
	public Color2[] C2s { get; set; }
}

public enum Color { Red, Green, Blue }

public enum Color2 { Teal, Ochre }

public enum Shade { Light, Dark }

public enum Shade2 { Light, Dark }

[Flags]
public enum Perms { None = 0, Read = 1, Write = 2, Exec = 4 }

public struct Point2
{
	public int X;
}

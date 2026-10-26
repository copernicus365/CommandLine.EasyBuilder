#:package System.CommandLine@2.0.0

// Probe: the static GetXOption() return-type check. Today's (CmdModelReflectionHelper.cs:111-112, 136)
// builds Option<PropType> with MakeGenericType and compares; the plan's compares the generic definition
// and the type argument directly. Every static below against every property, as Option and as Argument.

using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

using static System.Console;

WriteLine($"IsDynamicCodeSupported = {RuntimeFeature.IsDynamicCodeSupported}");

MethodInfo[] statics = Statics<Fixtures>();
PropertyInfo[] props = Props<Fixtures>();

int comparisons = 0;
int mismatches = 0;
List<string> matches = [];

foreach(Type genericDef in new[] { typeof(Option<>), typeof(Argument<>) }) {
	foreach(PropertyInfo p in props) {
		foreach(MethodInfo m in statics) {
			comparisons++;

			bool planSays = ReturnsSymbolOf(m.ReturnType, genericDef, p.PropertyType);

			if(RuntimeFeature.IsDynamicCodeSupported) {
				bool todaySays = TodaysCheck(m.ReturnType, genericDef, p.PropertyType);
				if(todaySays != planSays) {
					mismatches++;
					WriteLine($"MISMATCH  {m.Name} vs {p.Name} ({genericDef.Name}): today {todaySays}, plan {planSays}");
				}
			}

			if(planSays)
				matches.Add($"{genericDef.Name.TrimEnd('1', '`'),-9} {p.Name,-6} {p.PropertyType.Name,-9} <- {m.Name}()");
		}
	}
}

WriteLine($"{comparisons} comparisons, {matches.Count} match, {(RuntimeFeature.IsDynamicCodeSupported ? $"{mismatches} mismatches against today's check" : "today's check can't run in a native build")}");
foreach(string line in matches)
	WriteLine($"  {line}");

// today's check: CmdModelReflectionHelper.cs:111-112 and :136
#pragma warning disable IL3050
static bool TodaysCheck(Type returnType, Type genericDef, Type propertyType)
	=> returnType == genericDef.MakeGenericType(propertyType);
#pragma warning restore IL3050

// the plan's check
static bool ReturnsSymbolOf(Type returnType, Type genericDef, Type propertyType)
	=> returnType.IsGenericType
	&& returnType.GetGenericTypeDefinition() == genericDef
	&& returnType.GetGenericArguments()[0] == propertyType;

static MethodInfo[] Statics<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>()
	=> typeof(T).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);

static PropertyInfo[] Props<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>()
	=> typeof(T).GetProperties();

public class Fixtures
{
	public FileInfo File { get; set; }
	public int? Count { get; set; }
	public string[] Names { get; set; }
	public Color Mode { get; set; }

	public static Option<FileInfo> GetFileOption() => null;
	public static Argument<FileInfo> GetFileArgument() => null;
	public static Option<string> GetWrongTypeOption() => null;
	public static Option<int?> GetCountOption() => null;
	public static Option<int> GetCountNotNullableOption() => null;
	public static Option<string[]> GetNamesOption() => null;
	public static Option<Color> GetModeOption() => null;
	public static MyFileOption GetSubclassOption() => null;
	public static Option GetPlainOption() => null;
	public static Option<T> GetGenericOption<T>() => null;
}

public class MyFileOption() : Option<FileInfo>("--file");

public enum Color { Red, Green, Blue }

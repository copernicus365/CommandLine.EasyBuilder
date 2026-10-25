#:project ../CommandLine.EasyBuilder/CommandLine.EasyBuilder.csproj
#:property Nullable=disable

// aot-check: checks that CommandLine.EasyBuilder binds what System.CommandLine itself binds, the same way under
// the JIT and in a native AOT build, and that its refusals, RunOnce and Run behave as documented. Each check
// prints PASS or FAIL; any failure exits 1.
//
//   dotnet run tools/aot-check.cs -p:PublishAot=false               the JIT, dynamic code on: the reflection fallback runs
//   dotnet publish tools/aot-check.cs -o artifacts/aot-check        the native exe; then run artifacts/aot-check/aot-check.exe
//   dotnet run tools/aot-check.cs -p:PublishAot=false -- --legacy   against the library built with EASYBUILDER_LEGACY: the four
//                                                                   checks of what that build does the old way expect the old way
//   any of them with -- --dump-help                                 every command's help, to compare the JIT run's with the native one's

using System.CommandLine;
using System.Globalization;
using System.Runtime.CompilerServices;

using CommandLine.EasyBuilder;

using static System.Console;

// culture-free expectations: values parse, and System.CommandLine's messages read, the invariant way
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

bool legacy = args.Contains("--legacy");
bool dynamicCode = RuntimeFeature.IsDynamicCodeSupported;

// as a native app must: each property type beyond the built-in ones (Shade stays unregistered, on purpose)
AutoBuilder.RegisterTypes()
	.AddEnum<Color>()
	.AddEnum<Perms>()
	.Add<Point>();

RootCommand root = new("aot-check: a command per route EasyBuilder binds");
root.AddAutoCommand<TypesCmd>();
root.AddAutoCommand<ColorsCmd>();
root.AddAutoCommand<TintCmd>();
root.AddAutoCommand<DefaultsCmd>();
root.AddAutoCommand<FlagCmd>();
root.AddAutoCommand<FlagRestCmd>();
root.AddAutoCommand<WhereCmd>();
root.AddAutoCommand<InputCmd>();
root.AddAutoCommand(typeof(ModelCmd));
root.AddAutoCommand<ModelAsyncCmd>();
root.AddAutoCommand<SayCmd>();

RootCommand sayRoot = new("aot-check: a command set on the root");
sayRoot.SetAutoCommand<SayRootCmd>();

if(args.Contains("--dump-help")) {
	DumpHelp(root);
	DumpHelp(sayRoot);
	return 0;
}

int passed = 0;
int failed = 0;

CheckTable();
CheckEnums();
CheckDefaults();
CheckFlag();
CheckStatics();
CheckModel();
CheckRefusals();
await CheckRunOnceAndRun();

string mode = $"{(dynamicCode ? "dynamic code on" : "dynamic code off")}{(legacy ? ", legacy expectations" : "")}";

WriteLine();
WriteLine($"aot-check: {passed + failed} checks, {failed} failed ({mode})");

return failed == 0 ? 0 : 1;


// ---- the built-in table: every type System.CommandLine parses on its own ----

void CheckTable()
{
	const string guid1 = "01234567-89ab-cdef-0123-456789abcdef";
	const string guid2 = "00000000-0000-0000-0000-000000000001";

	TypesCmd.Last = null;
	Result given = Invoke(root, [
		"types",
		"--bool", "true", "--bool-n", "true", "--bool-a", "true", "--bool-a", "false",
		"--byte", "200", "--byte-n", "200", "--byte-a", "1", "--byte-a", "2",
		"--sbyte", "-5", "--sbyte-n", "-5", "--sbyte-a", "-1", "--sbyte-a", "2",
		"--short", "-300", "--short-n", "-300", "--short-a", "-300", "--short-a", "300",
		"--ushort", "60000", "--ushort-n", "60000", "--ushort-a", "1", "--ushort-a", "60000",
		"--int", "-7", "--int-n", "-7",
		"--uint", "4000000000", "--uint-n", "4000000000", "--uint-a", "1", "--uint-a", "4000000000",
		"--long", "-9000000000", "--long-n", "-9000000000",
		"--ulong", "18000000000000000000", "--ulong-n", "18000000000000000000", "--ulong-a", "1", "--ulong-a", "18000000000000000000",
		"--float", "1.5", "--float-n", "1.5", "--float-a", "1.5", "--float-a", "-2.25",
		"--double", "2.5", "--double-n", "2.5",
		"--decimal", "3.25", "--decimal-n", "3.25",
		"--datetime", "2026-10-25T12:00:00", "--datetime-n", "2026-10-25T12:00:00", "--datetime-a", "2026-10-25T12:00:00", "--datetime-a", "2026-10-26T08:30:00",
		"--dto", "2026-10-25T12:00:00+02:00", "--dto-n", "2026-10-25T12:00:00+02:00", "--dto-a", "2026-10-25T12:00:00+02:00", "--dto-a", "2026-10-26T08:30:00-05:00",
		"--dateonly", "2026-10-25", "--dateonly-n", "2026-10-25", "--dateonly-a", "2026-10-25", "--dateonly-a", "2026-12-31",
		"--timeonly", "13:45", "--timeonly-n", "13:45", "--timeonly-a", "13:45", "--timeonly-a", "08:05:30",
		"--timespan", "01:02:03", "--timespan-n", "01:02:03", "--timespan-a", "01:02:03", "--timespan-a", "2.00:00:00",
		"--guid", guid1, "--guid-n", guid1, "--guid-a", guid1, "--guid-a", guid2,
		"--string", "s", "--string-a", "a", "--string-a", "b",
		"--file", "a.txt", "--file-a", "a.txt", "--file-a", "b.txt",
		"--dir", "dir1", "--dir-a", "dir1", "--dir-a", "dir2",
		"--fsi", "whatever", "--fsi-a", "x1", "--fsi-a", "x2",
	]);
	Check("table: every type given a value, parsed and bound", given.Errors.Length == 0 && TypesCmd.Last != null, given.FirstError);

	TypesCmd t = TypesCmd.Last ?? new();

	DateTime dt1 = new(2026, 10, 25, 12, 0, 0);
	DateTime dt2 = new(2026, 10, 26, 8, 30, 0);
	DateTimeOffset dto1 = new(2026, 10, 25, 12, 0, 0, TimeSpan.FromHours(2));
	DateTimeOffset dto2 = new(2026, 10, 26, 8, 30, 0, TimeSpan.FromHours(-5));
	DateOnly date1 = new(2026, 10, 25);
	DateOnly date2 = new(2026, 12, 31);
	TimeOnly time1 = new(13, 45);
	TimeOnly time2 = new(8, 5, 30);
	TimeSpan span1 = new(1, 2, 3);
	TimeSpan span2 = TimeSpan.FromDays(2);
	Guid g1 = Guid.Parse(guid1);
	Guid g2 = Guid.Parse(guid2);

	(string Name, object Actual, object Expected)[] givenChecks = [
		("bool", t.Bool, true),
		("bool?", t.BoolN, true),
		("bool[]", t.BoolA, new[] { true, false }),
		("byte", t.Byte, (byte)200),
		("byte?", t.ByteN, (byte)200),
		("byte[]", t.ByteA, new byte[] { 1, 2 }),
		("sbyte", t.SByte, (sbyte)-5),
		("sbyte?", t.SByteN, (sbyte)-5),
		("sbyte[]", t.SByteA, new sbyte[] { -1, 2 }),
		("short", t.Short, (short)-300),
		("short?", t.ShortN, (short)-300),
		("short[]", t.ShortA, new short[] { -300, 300 }),
		("ushort", t.UShort, (ushort)60000),
		("ushort?", t.UShortN, (ushort)60000),
		("ushort[]", t.UShortA, new ushort[] { 1, 60000 }),
		("int", t.Int, -7),
		("int?", t.IntN, -7),
		("uint", t.UInt, 4000000000u),
		("uint?", t.UIntN, 4000000000u),
		("uint[]", t.UIntA, new uint[] { 1, 4000000000 }),
		("long", t.Long, -9000000000L),
		("long?", t.LongN, -9000000000L),
		("ulong", t.ULong, 18000000000000000000ul),
		("ulong?", t.ULongN, 18000000000000000000ul),
		("ulong[]", t.ULongA, new ulong[] { 1, 18000000000000000000 }),
		("float", t.Float, 1.5f),
		("float?", t.FloatN, 1.5f),
		("float[]", t.FloatA, new[] { 1.5f, -2.25f }),
		("double", t.Double, 2.5),
		("double?", t.DoubleN, 2.5),
		("decimal", t.Decimal, 3.25m),
		("decimal?", t.DecimalN, 3.25m),
		("DateTime", t.DateTime, dt1),
		("DateTime?", t.DateTimeN, dt1),
		("DateTime[]", t.DateTimeA, new[] { dt1, dt2 }),
		("DateTimeOffset", t.Dto, dto1),
		("DateTimeOffset?", t.DtoN, dto1),
		("DateTimeOffset[]", t.DtoA, new[] { dto1, dto2 }),
		("DateOnly", t.DateOnly, date1),
		("DateOnly?", t.DateOnlyN, date1),
		("DateOnly[]", t.DateOnlyA, new[] { date1, date2 }),
		("TimeOnly", t.TimeOnly, time1),
		("TimeOnly?", t.TimeOnlyN, time1),
		("TimeOnly[]", t.TimeOnlyA, new[] { time1, time2 }),
		("TimeSpan", t.TimeSpan, span1),
		("TimeSpan?", t.TimeSpanN, span1),
		("TimeSpan[]", t.TimeSpanA, new[] { span1, span2 }),
		("Guid", t.Guid, g1),
		("Guid?", t.GuidN, g1),
		("Guid[]", t.GuidA, new[] { g1, g2 }),
		("string", t.String, "s"),
		("string[]", t.StringA, new[] { "a", "b" }),
		("FileInfo", t.File, new FileInfo("a.txt")),
		("FileInfo[]", t.FileA, new[] { new FileInfo("a.txt"), new FileInfo("b.txt") }),
		("DirectoryInfo", t.Dir, new DirectoryInfo("dir1")),
		("DirectoryInfo[]", t.DirA, new[] { new DirectoryInfo("dir1"), new DirectoryInfo("dir2") }),
		("FileSystemInfo", t.Fsi, new FileInfo("whatever")),
		("FileSystemInfo[]", t.FsiA, new FileSystemInfo[] { new FileInfo("x1"), new FileInfo("x2") }),
	];

	foreach((string name, object actual, object expected) in givenChecks)
		CheckSame($"table, given: {name}", actual, expected);

	TypesCmd.Last = null;
	Result none = Invoke(root, "types");
	Check("table: no value given, parsed and bound", none.Errors.Length == 0 && TypesCmd.Last != null, none.FirstError);

	TypesCmd n = TypesCmd.Last ?? new();

	// with nothing given, each property reads what a plain System.CommandLine Option<T> of its type reads
	(string Name, object Actual, object Expected)[] noneChecks = [
		("bool", n.Bool, Plain<bool>()),
		("bool?", n.BoolN, Plain<bool?>()),
		("bool[]", n.BoolA, Plain<bool[]>()),
		("byte", n.Byte, Plain<byte>()),
		("byte?", n.ByteN, Plain<byte?>()),
		("byte[]", n.ByteA, Plain<byte[]>()),
		("sbyte", n.SByte, Plain<sbyte>()),
		("sbyte?", n.SByteN, Plain<sbyte?>()),
		("sbyte[]", n.SByteA, Plain<sbyte[]>()),
		("short", n.Short, Plain<short>()),
		("short?", n.ShortN, Plain<short?>()),
		("short[]", n.ShortA, Plain<short[]>()),
		("ushort", n.UShort, Plain<ushort>()),
		("ushort?", n.UShortN, Plain<ushort?>()),
		("ushort[]", n.UShortA, Plain<ushort[]>()),
		("int", n.Int, Plain<int>()),
		("int?", n.IntN, Plain<int?>()),
		("uint", n.UInt, Plain<uint>()),
		("uint?", n.UIntN, Plain<uint?>()),
		("uint[]", n.UIntA, Plain<uint[]>()),
		("long", n.Long, Plain<long>()),
		("long?", n.LongN, Plain<long?>()),
		("ulong", n.ULong, Plain<ulong>()),
		("ulong?", n.ULongN, Plain<ulong?>()),
		("ulong[]", n.ULongA, Plain<ulong[]>()),
		("float", n.Float, Plain<float>()),
		("float?", n.FloatN, Plain<float?>()),
		("float[]", n.FloatA, Plain<float[]>()),
		("double", n.Double, Plain<double>()),
		("double?", n.DoubleN, Plain<double?>()),
		("decimal", n.Decimal, Plain<decimal>()),
		("decimal?", n.DecimalN, Plain<decimal?>()),
		("DateTime", n.DateTime, Plain<DateTime>()),
		("DateTime?", n.DateTimeN, Plain<DateTime?>()),
		("DateTime[]", n.DateTimeA, Plain<DateTime[]>()),
		("DateTimeOffset", n.Dto, Plain<DateTimeOffset>()),
		("DateTimeOffset?", n.DtoN, Plain<DateTimeOffset?>()),
		("DateTimeOffset[]", n.DtoA, Plain<DateTimeOffset[]>()),
		("DateOnly", n.DateOnly, Plain<DateOnly>()),
		("DateOnly?", n.DateOnlyN, Plain<DateOnly?>()),
		("DateOnly[]", n.DateOnlyA, Plain<DateOnly[]>()),
		("TimeOnly", n.TimeOnly, Plain<TimeOnly>()),
		("TimeOnly?", n.TimeOnlyN, Plain<TimeOnly?>()),
		("TimeOnly[]", n.TimeOnlyA, Plain<TimeOnly[]>()),
		("TimeSpan", n.TimeSpan, Plain<TimeSpan>()),
		("TimeSpan?", n.TimeSpanN, Plain<TimeSpan?>()),
		("TimeSpan[]", n.TimeSpanA, Plain<TimeSpan[]>()),
		("Guid", n.Guid, Plain<Guid>()),
		("Guid?", n.GuidN, Plain<Guid?>()),
		("Guid[]", n.GuidA, Plain<Guid[]>()),
		("string", n.String, Plain<string>()),
		("string[]", n.StringA, Plain<string[]>()),
		("FileInfo", n.File, Plain<FileInfo>()),
		("FileInfo[]", n.FileA, Plain<FileInfo[]>()),
		("DirectoryInfo", n.Dir, Plain<DirectoryInfo>()),
		("DirectoryInfo[]", n.DirA, Plain<DirectoryInfo[]>()),
		("FileSystemInfo", n.Fsi, Plain<FileSystemInfo>()),
		("FileSystemInfo[]", n.FsiA, Plain<FileSystemInfo[]>()),
	];

	foreach((string name, object actual, object expected) in noneChecks)
		CheckSame($"table, none given: {name}", actual, expected);

	// double[] is EasyBuilder's own comma list, bound as an Option<string>
	TypesCmd.Last = null;
	Invoke(root, "types", "--delays", "1,2.5");
	CheckSame("table: double[] as a comma list", TypesCmd.Last?.Delays, new[] { 1.0, 2.5 });
}

// ---- enums, registered: what System.CommandLine's own Option<TEnum> gives ----

void CheckEnums()
{
	CheckColor("red", Color.Red);
	CheckColor("BLUE", Color.Blue);
	CheckColor("2", Color.Blue);
	CheckColor("99", (Color)99);

	ColorsCmd.Last = null;
	Invoke(root, "colors", "--perms", "read,write");
	CheckSame("enums: --perms read,write ([Flags])", ColorsCmd.Last?.Perms, Perms.Read | Perms.Write);

	ColorsCmd.Last = null;
	Invoke(root, "colors", "--perms", "3");
	CheckSame("enums: --perms 3 ([Flags], a number)", ColorsCmd.Last?.Perms, Perms.Read | Perms.Write);

	ColorsCmd.Last = null;
	Invoke(root, "colors", "--color-n", "green");
	CheckSame("enums: --color-n green (Color?)", ColorsCmd.Last?.ColorN, Color.Green);

	ColorsCmd.Last = null;
	Invoke(root, "colors", "--colors", "red", "--colors", "blue");
	CheckSame("enums: --colors red --colors blue (Color[])", ColorsCmd.Last?.Colors, new[] { Color.Red, Color.Blue });

	Result bogus = Invoke(root, "colors", "--color", "bogus");
	Check("enums: --color bogus refused", bogus.FirstError.StartsWith("Cannot parse argument 'bogus' for option '--color' as expected type"), bogus.FirstError);

	TintCmd.Last = null;
	Invoke(root, "tint", "Blue");
	CheckSame("enums: an enum argument", TintCmd.Last?.Tint, Color.Blue);

	string help = Invoke(root, "colors", "-h").Output;
	Check("enums: help lists the names, --color <Blue|Green|Red>", Line(help, "--color ").StartsWith("--color <Blue|Green|Red>"), Line(help, "--color "));
}

void CheckColor(string text, Color expected)
{
	ColorsCmd.Last = null;
	Invoke(root, "colors", "--color", text);
	CheckSame($"enums: --color {text}", ColorsCmd.Last?.Color, expected);
}

// ---- defaults from initializers and DefVal ----

void CheckDefaults()
{
	DefaultsCmd.Last = null;
	Invoke(root, "defaults");

	DefaultsCmd d = DefaultsCmd.Last;
	CheckSame("defaults: an int initializer", d?.Count, 5);
	CheckSame("defaults: an enum initializer", d?.Tint, Color.Green);
	CheckSame("defaults: a [Flags] initializer", d?.Perms, Perms.Read | Perms.Write);
	CheckSame("defaults: a DefVal", d?.Level, 3);

	DefaultsCmd.Last = null;
	Invoke(root, "defaults", "--count", "9", "--tint", "blue", "--level", "4");

	d = DefaultsCmd.Last;
	CheckSame("defaults: given beats an int initializer", d?.Count, 9);
	CheckSame("defaults: given beats an enum initializer", d?.Tint, Color.Blue);
	CheckSame("defaults: given beats a DefVal", d?.Level, 4);

	string help = Invoke(root, "defaults", "-h").Output;
	Check("defaults: help shows [default: 5]", Line(help, "--count").EndsWith("[default: 5]"), Line(help, "--count"));
	Check("defaults: help shows [default: Green]", Line(help, "--tint").EndsWith("[default: Green]"), Line(help, "--tint"));
	Check("defaults: help shows [default: Read, Write]", Line(help, "--perms").EndsWith("[default: Read, Write]"), Line(help, "--perms"));
	Check("defaults: help shows [default: 3]", Line(help, "--level").EndsWith("[default: 3]"), Line(help, "--level"));
}

// ---- the flag that takes no value, and the arity rules around it ----

void CheckFlag()
{
	FlagCmd.Last = null;
	Invoke(root, "flag", "--apply");
	CheckSame("flag: --apply", FlagCmd.Last?.Apply, true);

	FlagCmd.Last = null;
	Invoke(root, "flag");
	CheckSame("flag: absent", FlagCmd.Last?.Apply, false);

	Result refused = Invoke(root, "flag", "--apply", "false");
	Check("flag: --apply false refused", refused.FirstError == "Unrecognized command or argument 'false'.", refused.FirstError);

	FlagRestCmd.Last = null;
	Invoke(root, "flag-rest", "--apply", "false", "x");
	CheckSame("flag: a catch-all argument keeps what follows it", FlagRestCmd.Last?.Rest, new[] { "false", "x" });
	CheckSame("flag: and the flag is set", FlagRestCmd.Last?.Apply, true);

	FlagCmd.Last = null;
	Invoke(root, "flag", "--light", "false");
	CheckSame("flag: a bool with neither arity set still takes a value", FlagCmd.Last?.Light, false);

	Command flagCmd = root.Subcommands.First(c => c.Name == "flag");
	CheckArity("flag: MaxArity = 0 is 0..0", flagCmd, "--apply", 0, 0);
	CheckArity("flag: neither set leaves System.CommandLine's own, 0..1 for a bool", flagCmd, "--light", 0, 1);
	CheckArity("flag: MaxArity = 3 alone is 0..3, as before", flagCmd, "--fg", 0, 3);

	string help = Invoke(root, "flag", "-h").Output;
	string applyLine = Line(help, "--apply");
	Check("flag: help shows --apply with no value placeholder", applyLine.StartsWith("-a, --apply") && !applyLine.Contains('<'), applyLine);
}

void CheckArity(string name, Command cmd, string optionName, int min, int max)
{
	ArgumentArity arity = cmd.Options.First(o => o.Name == optionName).Arity;
	Check(name, arity.MinimumNumberOfValues == min && arity.MaximumNumberOfValues == max, $"{arity.MinimumNumberOfValues}..{arity.MaximumNumberOfValues}");
}

// ---- statics: an Option<T> supplied by a static Get{PropName}Option() ----

void CheckStatics()
{
	WhereCmd.Last = null;
	Invoke(root, "where", "--where", "3,4");
	CheckSame("statics: a registered struct, from GetWhereOption() and its parser", WhereCmd.Last?.Where, new Point(3, 4));

	InputCmd.Last = null;
	Invoke(root, "input", "--input", "a.txt");
	CheckSame("statics: on the base class that declares the property", InputCmd.Last?.Input, new FileInfo("a.txt"));

	// the one new case: a static on the command class, for a property its base class declares with none
	string name = legacy
		? "statics: on the command class, for an inherited property: not found, the old way"
		: "statics: on the command class, for an inherited property: found";

	try {
		RootCommand inheritRoot = new("inherit");
		inheritRoot.AddAutoCommand<InheritCmd>();

		InheritCmd.Last = null;
		Invoke(inheritRoot, "inherit", "--source", "b.txt");
		Check(name, !legacy && Same(InheritCmd.Last?.Source, new FileInfo("b.txt")), $"found: {Show(InheritCmd.Last?.Source)}");
	}
	catch(InvalidOperationException ex) {
		Check(name, legacy && ex.Message.Contains("no valid static method found"), ex.Message);
	}
}

// ---- the model: arguments, ParseResult, Handle / HandleAsync, the Type and SetAutoCommand routes ----

void CheckModel()
{
	ModelCmd.Last = null;
	Invoke(root, "model", "hello");
	CheckSame("model: a string argument, on a command added by AddAutoCommand(Type)", ModelCmd.Last?.Text, "hello");
	Check("model: ParseResult set through its private setter", ModelCmd.Last?.ParseResult != null);

	ModelAsyncCmd.Last = null;
	Invoke(root, "model-async", "--n", "4");
	CheckSame("model: HandleAsync", ModelAsyncCmd.Last?.N, 4);

	SayRootCmd.Last = null;
	Invoke(sayRoot, "-p", "x");
	CheckSame("model: SetAutoCommand<T>() on a root", SayRootCmd.Last?.P, "x");
}

// ---- refusals, when the command tree is built ----

void CheckRefusals()
{
	// Shade is registered nowhere: with dynamic code on, the reflection fallback binds it; with it off, the tree refuses it
	string shadeName = dynamicCode
		? "refusals: an unregistered enum binds, dynamic code on"
		: "refusals: an unregistered enum is refused, naming AddEnum<Shade>(), dynamic code off";

	try {
		RootCommand shadeRoot = new("shade");
		shadeRoot.AddAutoCommand<ShadeCmd>();

		ShadeCmd.Last = null;
		Invoke(shadeRoot, "shade", "--shade", "dark");
		Check(shadeName, dynamicCode && ShadeCmd.Last?.Shade == Shade.Dark, $"bound: {Show(ShadeCmd.Last?.Shade)}");
	}
	catch(InvalidOperationException ex) {
		Check(shadeName, !dynamicCode && ex.Message.Contains("`AutoBuilder.RegisterTypes().AddEnum<Shade>();`"), ex.Message);
	}

	Refused("refusals: MaxArity = 0 on a string",
		() => new RootCommand().AddAutoCommand<NoValueStringCmd>(),
		typeof(InvalidOperationException),
		"Property 'Name' takes no value (MaxArity = 0)");

	Refused("refusals: a no-value flag defaulting to true",
		() => new RootCommand().AddAutoCommand<FlagTrueCmd>(),
		typeof(InvalidOperationException),
		"Property 'Apply' takes no value (MaxArity = 0), so its default can't be true");

	// a default of the wrong type, refused as the tree is built; the legacy build refuses it in Expression.Constant
	Type castEx = legacy ? typeof(ArgumentException) : typeof(InvalidCastException);
	string how = legacy ? ", ArgumentException, the old way" : ", InvalidCastException";

	Refused($"refusals: DefVal = 42 on a double{how}",
		() => new RootCommand().AddAutoCommand<DoubleFromIntCmd>(),
		castEx,
		legacy ? "Argument types do not match" : "Property 'Delay' has the default 42 (Int32)");

	Refused($"refusals: DefVal = 1 on an enum{how}",
		() => new RootCommand().AddAutoCommand<EnumFromIntCmd>(),
		castEx,
		legacy ? "Argument types do not match" : "Property 'Tint' has the default 1 (Int32)");

	Refused($"refusals: DefVal = Color.Blue on an int{how}",
		() => new RootCommand().AddAutoCommand<IntFromEnumCmd>(),
		castEx,
		legacy ? "Argument types do not match" : "Property 'Count' has the default Blue (Color)");
}

void Refused(string name, Action build, Type expected, string expectedText)
{
	try {
		build();
		Check(name, false, "built; expected a refusal");
	}
	catch(Exception ex) {
		bool ok = ex.GetType() == expected && ex.Message.Contains(expectedText);
		Check(name, ok, $"{ex.GetType().Name}: {ex.Message}");
	}
}

// ---- RunOnce and Run: the args parsed as given, and parserConfig ----

async Task CheckRunOnceAndRun()
{
	const string quoted = "say \"hi\" to them";

	SayCmd.Last = null;
	(int exit, string output) = await Capture(() => BasicCLI.RunOnce(root, ["say", "-p", quoted]));

	if(legacy)
		Check("RunOnce: a value with quotes is re-split, the old way", exit == 1 && SayCmd.Last == null, $"exit {exit}, bound: {SayCmd.Last?.P}");
	else
		Check("RunOnce: a value with quotes binds as given", exit == 0 && SayCmd.Last?.P == quoted, $"exit {exit}, bound: {SayCmd.Last?.P}; {output.Trim()}");

	(exit, output) = await Capture(() => BasicCLI.RunOnce(root, ["say", "-p", "@team please review"]));
	Check("RunOnce: an @ value is read as a response file by default", exit == 1 && output.Contains("Response file not found"), output.Trim());

	ParserConfiguration noResponseFiles = new() { ResponseFileTokenReplacer = null };

	SayCmd.Last = null;
	(exit, output) = await Capture(() => BasicCLI.RunOnce(root, ["say", "-p", "@team please review"], parserConfig: noResponseFiles));
	Check("RunOnce: parserConfig turns response files off", exit == 0 && SayCmd.Last?.P == "@team please review", $"exit {exit}, bound: {SayCmd.Last?.P}; {output.Trim()}");

	(exit, output) = await Capture(() => BasicCLI.RunOnce(root, []));
	Check("RunOnce: no arguments runs -h", exit == 0 && output.Contains("Usage:"), $"exit {exit}");

	// Run in REPL mode: the first command from the args, then the REPL, which reads exit and ends
	TextReader input = In;
	SetIn(new StringReader("exit" + Environment.NewLine));

	SayCmd.Last = null;
	(exit, output) = await Capture(async () => {
		await BasicCLI.Run(root, ["--repl", "say", "-p", quoted]);
		return 0;
	});

	SetIn(input);

	if(legacy)
		Check("Run: the REPL's first command is re-split, the old way", SayCmd.Last == null && output.Contains("Error:"), $"bound: {SayCmd.Last?.P}");
	else
		Check("Run: the REPL's first command binds a value with quotes as given", SayCmd.Last?.P == quoted, $"bound: {SayCmd.Last?.P}; {output.Trim()}");
}

// ---- helpers ----

void Check(string name, bool ok, string detail = null)
{
	if(ok)
		passed++;
	else
		failed++;

	string note = ok || string.IsNullOrEmpty(detail) ? "" : $"  ({detail})";
	WriteLine($"{(ok ? "PASS" : "FAIL")}  {name}{note}");
}

void CheckSame(string name, object actual, object expected)
	=> Check(name, Same(actual, expected), $"got {Show(actual)}, expected {Show(expected)}");

void DumpHelp(RootCommand rootCmd)
{
	WriteLine($"===== {rootCmd.Description}: -h =====");
	Write(Invoke(rootCmd, "-h").Output);

	foreach(Command sub in rootCmd.Subcommands) {
		WriteLine($"===== {rootCmd.Description}: {sub.Name} -h =====");
		Write(Invoke(rootCmd, sub.Name, "-h").Output);
	}
}

static Result Invoke(RootCommand rootCmd, params string[] cmdArgs)
{
	ParseResult res = rootCmd.Parse(cmdArgs);
	if(res.Errors.Count > 0)
		return new(1, "", [.. res.Errors.Select(e => e.Message)]);

	StringWriter output = new();
	InvocationConfiguration config = new() { Output = output, Error = output };

	int exit = res.Invoke(config);
	return new(exit, output.ToString(), []);
}

// what a plain System.CommandLine Option<T> reads when nothing is given
static T Plain<T>()
{
	Option<T> option = new("--plain");
	RootCommand plainRoot = new();
	plainRoot.Options.Add(option);

	return plainRoot.Parse([]).GetValue(option);
}

static async Task<(int Exit, string Output)> Capture(Func<Task<int>> run)
{
	TextWriter original = Out;
	StringWriter captured = new();
	SetOut(captured);

	try {
		int exit = await run();
		return (exit, captured.ToString());
	}
	finally {
		SetOut(original);
	}
}

static bool Same(object a, object b)
{
	if(a is null || b is null)
		return a is null && b is null;

	if(a is FileSystemInfo fa && b is FileSystemInfo fb)
		return fa.Name == fb.Name;

	if(a is Array arrA && b is Array arrB) {
		if(arrA.Length != arrB.Length)
			return false;

		for(int i = 0; i < arrA.Length; i++)
			if(!Same(arrA.GetValue(i), arrB.GetValue(i)))
				return false;

		return true;
	}

	return a.Equals(b);
}

static string Show(object v) => v switch {
	null => "null",
	FileSystemInfo f => f.Name,
	Array a => "[" + string.Join(", ", a.Cast<object>().Select(Show)) + "]",
	_ => v.ToString(),
};

// the first line of text containing needle, trimmed
static string Line(string text, string needle)
	=> text.Split('\n').FirstOrDefault(l => l.Contains(needle))?.Trim() ?? "(no such line)";


record Result(int Exit, string Output, string[] Errors)
{
	public string FirstError => Errors.Length > 0 ? Errors[0] : "";
}

public enum Color { Red, Green, Blue }

[Flags]
public enum Perms { None = 0, Read = 1, Write = 2, Exec = 4 }

public enum Shade { Light, Dark }

public readonly record struct Point(int X, int Y);

[Command("types", "One option per built-in type")]
public class TypesCmd
{
	[Option("--bool")] public bool Bool { get; set; }
	[Option("--bool-n")] public bool? BoolN { get; set; }
	[Option("--bool-a")] public bool[] BoolA { get; set; }

	[Option("--byte")] public byte Byte { get; set; }
	[Option("--byte-n")] public byte? ByteN { get; set; }
	[Option("--byte-a")] public byte[] ByteA { get; set; }

	[Option("--sbyte")] public sbyte SByte { get; set; }
	[Option("--sbyte-n")] public sbyte? SByteN { get; set; }
	[Option("--sbyte-a")] public sbyte[] SByteA { get; set; }

	[Option("--short")] public short Short { get; set; }
	[Option("--short-n")] public short? ShortN { get; set; }
	[Option("--short-a")] public short[] ShortA { get; set; }

	[Option("--ushort")] public ushort UShort { get; set; }
	[Option("--ushort-n")] public ushort? UShortN { get; set; }
	[Option("--ushort-a")] public ushort[] UShortA { get; set; }

	[Option("--int")] public int Int { get; set; }
	[Option("--int-n")] public int? IntN { get; set; }

	[Option("--uint")] public uint UInt { get; set; }
	[Option("--uint-n")] public uint? UIntN { get; set; }
	[Option("--uint-a")] public uint[] UIntA { get; set; }

	[Option("--long")] public long Long { get; set; }
	[Option("--long-n")] public long? LongN { get; set; }

	[Option("--ulong")] public ulong ULong { get; set; }
	[Option("--ulong-n")] public ulong? ULongN { get; set; }
	[Option("--ulong-a")] public ulong[] ULongA { get; set; }

	[Option("--float")] public float Float { get; set; }
	[Option("--float-n")] public float? FloatN { get; set; }
	[Option("--float-a")] public float[] FloatA { get; set; }

	[Option("--double")] public double Double { get; set; }
	[Option("--double-n")] public double? DoubleN { get; set; }

	[Option("--decimal")] public decimal Decimal { get; set; }
	[Option("--decimal-n")] public decimal? DecimalN { get; set; }

	[Option("--datetime")] public DateTime DateTime { get; set; }
	[Option("--datetime-n")] public DateTime? DateTimeN { get; set; }
	[Option("--datetime-a")] public DateTime[] DateTimeA { get; set; }

	[Option("--dto")] public DateTimeOffset Dto { get; set; }
	[Option("--dto-n")] public DateTimeOffset? DtoN { get; set; }
	[Option("--dto-a")] public DateTimeOffset[] DtoA { get; set; }

	[Option("--dateonly")] public DateOnly DateOnly { get; set; }
	[Option("--dateonly-n")] public DateOnly? DateOnlyN { get; set; }
	[Option("--dateonly-a")] public DateOnly[] DateOnlyA { get; set; }

	[Option("--timeonly")] public TimeOnly TimeOnly { get; set; }
	[Option("--timeonly-n")] public TimeOnly? TimeOnlyN { get; set; }
	[Option("--timeonly-a")] public TimeOnly[] TimeOnlyA { get; set; }

	[Option("--timespan")] public TimeSpan TimeSpan { get; set; }
	[Option("--timespan-n")] public TimeSpan? TimeSpanN { get; set; }
	[Option("--timespan-a")] public TimeSpan[] TimeSpanA { get; set; }

	[Option("--guid")] public Guid Guid { get; set; }
	[Option("--guid-n")] public Guid? GuidN { get; set; }
	[Option("--guid-a")] public Guid[] GuidA { get; set; }

	[Option("--string")] public string String { get; set; }
	[Option("--string-a")] public string[] StringA { get; set; }

	[Option("--file")] public FileInfo File { get; set; }
	[Option("--file-a")] public FileInfo[] FileA { get; set; }

	[Option("--dir")] public DirectoryInfo Dir { get; set; }
	[Option("--dir-a")] public DirectoryInfo[] DirA { get; set; }

	[Option("--fsi")] public FileSystemInfo Fsi { get; set; }
	[Option("--fsi-a")] public FileSystemInfo[] FsiA { get; set; }

	// not a table type: EasyBuilder's own comma list
	[Option("--delays")] public double[] Delays { get; set; }

	public void Handle() => Last = this;

	public static TypesCmd Last;
}

[Command("colors", "Enums, registered")]
public class ColorsCmd
{
	[Option("--color")]
	public Color Color { get; set; }

	[Option("--color-n")]
	public Color? ColorN { get; set; }

	[Option("--colors")]
	public Color[] Colors { get; set; }

	[Option("--perms")]
	public Perms Perms { get; set; }

	public void Handle() => Last = this;

	public static ColorsCmd Last;
}

[Command("tint", "An enum argument")]
public class TintCmd
{
	[Argument("tint")]
	public Color Tint { get; set; }

	public void Handle() => Last = this;

	public static TintCmd Last;
}

[Command("defaults", "Defaults from initializers and a DefVal")]
public class DefaultsCmd
{
	[Option("--count")]
	public int Count { get; set; } = 5;

	[Option("--tint")]
	public Color Tint { get; set; } = Color.Green;

	[Option("--perms")]
	public Perms Perms { get; set; } = Perms.Read | Perms.Write;

	[Option("--level", DefVal = 3)]
	public int Level { get; set; }

	public void Handle() => Last = this;

	public static DefaultsCmd Last;
}

[Command("flag", "A flag that takes no value, and the arity rules")]
public class FlagCmd
{
	[Option("--apply", "-a", "Apply the plan", MaxArity = 0)]
	public bool Apply { get; set; }

	[Option("--light", description: "A bool with neither arity set")]
	public bool Light { get; set; }

	[Option("--fg", description: "MaxArity alone, as the tutorial's --fgcolor", MaxArity = 3)]
	public Color Fg { get; set; }

	public void Handle() => Last = this;

	public static FlagCmd Last;
}

[Command("flag-rest", "A flag that takes no value, before a catch-all argument")]
public class FlagRestCmd
{
	[Option("--apply", MaxArity = 0)]
	public bool Apply { get; set; }

	[Argument("rest")]
	public string[] Rest { get; set; }

	public void Handle() => Last = this;

	public static FlagRestCmd Last;
}

[Command("where", "A registered struct, supplied by a static with its own parser")]
public class WhereCmd
{
	[Option("")]
	public Point Where { get; set; }

	public static Option<Point> GetWhereOption() => new("--where") {
		CustomParser = result => {
			string[] parts = result.Tokens.Single().Value.Split(',');
			return new Point(int.Parse(parts[0]), int.Parse(parts[1]));
		},
	};

	public void Handle() => Last = this;

	public static WhereCmd Last;
}

public class InputBase
{
	[Option("")]
	public FileInfo Input { get; set; }

	public static Option<FileInfo> GetInputOption() => new("--input");
}

[Command("input", "A static on the base class that declares the property, as the tutorial's FileBase")]
public class InputCmd : InputBase
{
	public void Handle() => Last = this;

	public static InputCmd Last;
}

public class InheritBase
{
	[Option("")]
	public FileInfo Source { get; set; }
}

[Command("inherit", "A static on the command class, for a property its base class declares with none")]
public class InheritCmd : InheritBase
{
	public static Option<FileInfo> GetSourceOption() => new("--source");

	public void Handle() => Last = this;

	public static InheritCmd Last;
}

[Command("model", "A string argument, and ParseResult with a private setter")]
public class ModelCmd
{
	[Argument("text", "Some text")]
	public string Text { get; set; }

	public ParseResult ParseResult { get; private set; }

	public void Handle() => Last = this;

	public static ModelCmd Last;
}

[Command("model-async", "HandleAsync")]
public class ModelAsyncCmd
{
	[Option("--n")]
	public int N { get; set; }

	public async Task HandleAsync()
	{
		await Task.Yield();
		Last = this;
	}

	public static ModelAsyncCmd Last;
}

[Command("say", "A string option, for RunOnce and Run")]
public class SayCmd
{
	[Option("-p")]
	public string P { get; set; }

	public void Handle() => Last = this;

	public static SayCmd Last;
}

[Command("say-root", "A string option, on a command set on the root")]
public class SayRootCmd
{
	[Option("-p")]
	public string P { get; set; }

	public void Handle() => Last = this;

	public static SayRootCmd Last;
}

[Command("shade", "An enum registered nowhere")]
public class ShadeCmd
{
	[Option("--shade")]
	public Shade Shade { get; set; }

	public void Handle() => Last = this;

	public static ShadeCmd Last;
}

[Command("no-value-string", "MaxArity = 0 on a string")]
public class NoValueStringCmd
{
	[Option("--name", MaxArity = 0)]
	public string Name { get; set; }
}

[Command("flag-true", "A no-value flag defaulting to true")]
public class FlagTrueCmd
{
	[Option("--apply", MaxArity = 0, DefVal = true)]
	public bool Apply { get; set; }
}

[Command("double-from-int", "DefVal = 42 on a double")]
public class DoubleFromIntCmd
{
	[Option("--delay", DefVal = 42)]
	public double Delay { get; set; }
}

[Command("enum-from-int", "DefVal = 1 on an enum")]
public class EnumFromIntCmd
{
	[Option("--tint", DefVal = 1)]
	public Color Tint { get; set; }
}

[Command("int-from-enum", "DefVal = Color.Blue on an int")]
public class IntFromEnumCmd
{
	[Option("--count", DefVal = Color.Blue)]
	public int Count { get; set; }
}

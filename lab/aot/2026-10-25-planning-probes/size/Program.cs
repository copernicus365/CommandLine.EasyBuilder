using System.CommandLine;

using GuardProbe;

using static System.Console;

// Probe: native size cost of typed-binder table entries. LEAN = today's special-cased types;
// WIDE adds the other BCL value types System.CommandLine parses natively.

Dictionary<Type, IPropBinder> table = new() {
	[typeof(bool)] = new PropBinder<bool>(),
	[typeof(bool?)] = new PropBinder<bool?>(),
	[typeof(int)] = new PropBinder<int>(),
	[typeof(int?)] = new PropBinder<int?>(),
	[typeof(long)] = new PropBinder<long>(),
	[typeof(long?)] = new PropBinder<long?>(),
	[typeof(double)] = new PropBinder<double>(),
	[typeof(double?)] = new PropBinder<double?>(),
	[typeof(decimal)] = new PropBinder<decimal>(),
	[typeof(decimal?)] = new PropBinder<decimal?>(),
	[typeof(string)] = new PropBinder<string>(),
	[typeof(string[])] = new PropBinder<string[]>(),
	[typeof(FileInfo)] = new PropBinder<FileInfo>(),
	[typeof(DirectoryInfo)] = new PropBinder<DirectoryInfo>(),
	[typeof(Uri)] = new PropBinder<Uri>(),
#if WIDE
	[typeof(float)] = new PropBinder<float>(),
	[typeof(float?)] = new PropBinder<float?>(),
	[typeof(byte)] = new PropBinder<byte>(),
	[typeof(byte?)] = new PropBinder<byte?>(),
	[typeof(sbyte)] = new PropBinder<sbyte>(),
	[typeof(sbyte?)] = new PropBinder<sbyte?>(),
	[typeof(short)] = new PropBinder<short>(),
	[typeof(short?)] = new PropBinder<short?>(),
	[typeof(ushort)] = new PropBinder<ushort>(),
	[typeof(ushort?)] = new PropBinder<ushort?>(),
	[typeof(uint)] = new PropBinder<uint>(),
	[typeof(uint?)] = new PropBinder<uint?>(),
	[typeof(ulong)] = new PropBinder<ulong>(),
	[typeof(ulong?)] = new PropBinder<ulong?>(),
	[typeof(DateTime)] = new PropBinder<DateTime>(),
	[typeof(DateTime?)] = new PropBinder<DateTime?>(),
	[typeof(DateTimeOffset)] = new PropBinder<DateTimeOffset>(),
	[typeof(DateTimeOffset?)] = new PropBinder<DateTimeOffset?>(),
	[typeof(TimeSpan)] = new PropBinder<TimeSpan>(),
	[typeof(TimeSpan?)] = new PropBinder<TimeSpan?>(),
	[typeof(Guid)] = new PropBinder<Guid>(),
	[typeof(Guid?)] = new PropBinder<Guid?>(),
	[typeof(DateOnly)] = new PropBinder<DateOnly>(),
	[typeof(DateOnly?)] = new PropBinder<DateOnly?>(),
	[typeof(TimeOnly)] = new PropBinder<TimeOnly>(),
	[typeof(TimeOnly?)] = new PropBinder<TimeOnly?>(),
#endif
#if ARRAYS
	[typeof(bool[])] = new PropBinder<bool[]>(),
	[typeof(byte[])] = new PropBinder<byte[]>(),
	[typeof(sbyte[])] = new PropBinder<sbyte[]>(),
	[typeof(short[])] = new PropBinder<short[]>(),
	[typeof(ushort[])] = new PropBinder<ushort[]>(),
	[typeof(uint[])] = new PropBinder<uint[]>(),
	[typeof(ulong[])] = new PropBinder<ulong[]>(),
	[typeof(float[])] = new PropBinder<float[]>(),
	[typeof(DateTime[])] = new PropBinder<DateTime[]>(),
	[typeof(DateTimeOffset[])] = new PropBinder<DateTimeOffset[]>(),
	[typeof(DateOnly[])] = new PropBinder<DateOnly[]>(),
	[typeof(TimeOnly[])] = new PropBinder<TimeOnly[]>(),
	[typeof(TimeSpan[])] = new PropBinder<TimeSpan[]>(),
	[typeof(Guid[])] = new PropBinder<Guid[]>(),
	[typeof(FileInfo[])] = new PropBinder<FileInfo[]>(),
	[typeof(DirectoryInfo[])] = new PropBinder<DirectoryInfo[]>(),
	[typeof(FileSystemInfo[])] = new PropBinder<FileSystemInfo[]>(),
#endif
};

// Use every interface member through the table, as the library would
string typeName = args.Length > 0 ? args[0] : "System.Int32";
IPropBinder b = table.First(kv => kv.Key.FullName == typeName).Value;

RootCommand root = new("size");
Option opt = b.CreateOption("--x");
b.SetDefault(opt, b.DefaultOfT);
root.Options.Add(opt);

ParseResult r = root.Parse(args.Skip(1).ToArray());
WriteLine($"{table.Count} entries; {typeName} -> {b.GetValue(r, opt) ?? "null"}");

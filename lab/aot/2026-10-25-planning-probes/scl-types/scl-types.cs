#:package System.CommandLine@2.0.0
#:property PublishAot=false

// Probe: which property types System.CommandLine 2.0.0 parses on its own (no CustomParser).

using System.CommandLine;
using System.Net;
using System.Numerics;

using static System.Console;

WriteLine("--- value types ---");
Try<bool>("true");
Try<byte>("200");
Try<sbyte>("-5");
Try<short>("-300");
Try<ushort>("60000");
Try<int>("-7");
Try<uint>("4000000000");
Try<long>("-9000000000");
Try<ulong>("18000000000000000000");
Try<float>("1.5");
Try<double>("2.5");
Try<decimal>("3.25");
Try<char>("x");
Try<Half>("1.5");
Try<Int128>("170141183460469231731687303715884105727");
Try<UInt128>("1");
Try<nint>("5");
Try<nuint>("6");
Try<BigInteger>("123456789012345678901234567890");
Try<DateTime>("2026-10-25T12:00:00");
Try<DateTimeOffset>("2026-10-25T12:00:00+02:00");
Try<DateOnly>("2026-10-25");
Try<TimeOnly>("13:45");
Try<TimeSpan>("01:02:03");
Try<Guid>("01234567-89ab-cdef-0123-456789abcdef");

WriteLine();
WriteLine("--- nullable (spot check) ---");
Try<int?>("4");
Try<DateOnly?>("2026-10-25");

WriteLine();
WriteLine("--- reference types ---");
Try<string>("s");
Try<FileInfo>("a.txt");
Try<DirectoryInfo>("dir");
Try<FileSystemInfo>("whatever");
Try<Uri>("https://example.com/x");
Try<Version>("1.2.3");
Try<IPAddress>("127.0.0.1");

static void Try<T>(string sample)
{
	Option<T> opt = new("--x");
	RootCommand root = new("probe");
	root.Options.Add(opt);

	ParseResult r = root.Parse(["--x", sample]);
	string name = typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>)
		? Nullable.GetUnderlyingType(typeof(T)).Name + "?"
		: typeof(T).Name;

	if(r.Errors.Count > 0) {
		WriteLine($"{name,-16} NO   parse error: {r.Errors[0].Message}");
		return;
	}

	try {
		WriteLine($"{name,-16} yes  {r.GetValue(opt)}");
	}
	catch(Exception ex) {
		WriteLine($"{name,-16} NO   no parse error, but GetValue threw {ex.GetType().Name}: {ex.Message}");
	}
}

#:package System.CommandLine@2.0.0
#:property PublishAot=false

// Probe: BasicCLI.RunOnce joins args into one string and System.CommandLine splits it again.
// JoinArgs below is copied verbatim from CommandLine.EasyBuilder/BasicCLI.cs:157-174.

using System.CommandLine;
using System.Text;

using static System.Console;

RootCommand root = new("probe");
Option<string> prompt = new("-p");
Argument<string[]> rest = new("rest") { Arity = ArgumentArity.ZeroOrMore };
root.Options.Add(prompt);
root.Arguments.Add(rest);

string[][] cases = [
	["-p", "say \"hi\" to them"],
	["-p", "C:\\dir with space\\"],
	["-p", ""],
	["a", "--", "-p", "x y"],
	["-p", "@team please review"],
];

foreach(string[] input in cases) {
	string joined = JoinArgs(input);
	ParseResult viaString = root.Parse(joined);
	ParseResult viaArray = root.Parse(input);

	WriteLine($"args    {Show(input)}");
	WriteLine($"joined  {joined}");
	WriteLine($"  RunOnce today (string)  -> {Bound(viaString)}");
	WriteLine($"  Parse(args)   (array)   -> {Bound(viaArray)}");
	WriteLine();
}

string Bound(ParseResult r)
	=> r.Errors.Count > 0
		? "ERROR: " + string.Join(" / ", r.Errors.Select(e => e.Message))
		: $"-p {Q(r.GetValue(prompt))}  rest {Show(r.GetValue(rest) ?? [])}";

static string Q(string s) => s == null ? "null" : $"«{s}»";

static string Show(string[] a) => "[" + string.Join(", ", a.Select(Q)) + "]";

static string JoinArgs(string[] args)
{
	StringBuilder sb = new();
	for(int i = 0; i < args.Length; i++) {
		if(i > 0) sb.Append(' ');
		string a = args[i] ?? "";
		bool needsQuotes = a.Length == 0 || a.AsSpan().IndexOfAny(' ', '\t', '"') >= 0;
		if(needsQuotes) {
			sb.Append('"');
			sb.Append(a.Replace("\"", "\\\""));
			sb.Append('"');
		}
		else {
			sb.Append(a);
		}
	}
	return sb.ToString();
}

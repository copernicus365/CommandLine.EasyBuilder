#:package System.CommandLine@2.0.0
#:property PublishAot=false

// Probe: how System.CommandLine 2.0.0 itself treats enum options and bool flags.
// Evidence for the EasyBuilder native-AOT plan; changes nothing anywhere.

using System.CommandLine;
using System.CommandLine.Parsing;

using static System.Console;

RootCommand root = new("probe");

// --- enums ---

Command enums = new("enums");

Option<Color> color = new("--color") { Description = "Option<Color>" };
Option<Color?> colorN = new("--color-n") { Description = "Option<Color?>" };
Option<Color[]> colors = new("--colors") { Description = "Option<Color[]>" };
Option<Perms> perms = new("--perms") { Description = "Option<Perms> [Flags]" };
Option<Color> colorD = new("--color-d") { Description = "Option<Color>, default Green", DefaultValueFactory = _ => Color.Green };
Option<Perms> permsD = new("--perms-d") { Description = "Option<Perms>, default Read|Write", DefaultValueFactory = _ => Perms.Read | Perms.Write };
Option<Color[]> colorsD = new("--colors-d") { Description = "Option<Color[]>, default [Red, Blue]", DefaultValueFactory = _ => [Color.Red, Color.Blue] };

Option<string> colorS = new("--color-s") { Description = "Option<string> + names as completions" };
colorS.CompletionSources.Add(Enum.GetNames<Color>());

Option<string[]> colorsS = new("--colors-s") { Description = "Option<string[]> + names as completions" };
colorsS.CompletionSources.Add(Enum.GetNames<Color>());

Option<string> colorSD = new("--color-sd") { Description = "Option<string> + completions, default \"Green\"", DefaultValueFactory = _ => "Green" };
colorSD.CompletionSources.Add(Enum.GetNames<Color>());

Argument<Color> colorArg = new("shade") { Description = "Argument<Color>", Arity = ArgumentArity.ZeroOrOne };

foreach(Option o in new Option[] { color, colorN, colors, perms, colorD, permsD, colorsD, colorS, colorsS, colorSD })
	enums.Options.Add(o);

enums.Arguments.Add(colorArg);
root.Subcommands.Add(enums);

// --- bool flags ---

Command flags = new("flags");
Option<bool> apply = new("--apply") { Description = "Option<bool>, default arity" };
Option<bool> flag0 = new("--flag0") { Description = "Option<bool>, Arity = Zero" };
Option<bool?> applyN = new("--apply-n") { Description = "Option<bool?>, default arity" };
Option<bool?> flag0N = new("--flag0-n") { Description = "Option<bool?>, Arity = Zero" };
flag0.Arity = ArgumentArity.Zero;
flag0N.Arity = ArgumentArity.Zero;
Argument<string> name = new("name");
flags.Options.Add(apply);
flags.Options.Add(flag0);
flags.Options.Add(applyN);
flags.Options.Add(flag0N);
flags.Arguments.Add(name);
root.Subcommands.Add(flags);

Command flagsRest = new("flags-rest");
Option<bool> flag0R = new("--flag0") { Arity = ArgumentArity.Zero };
Argument<string[]> rest = new("rest") { Arity = ArgumentArity.ZeroOrMore };
flagsRest.Options.Add(flag0R);
flagsRest.Arguments.Add(rest);
root.Subcommands.Add(flagsRest);

// --- zero arity on non-bool options ---

Command zeros = new("zeros");
Option<string> s0 = new("--s0") { Arity = ArgumentArity.Zero };
Option<int> i0 = new("--i0") { Arity = ArgumentArity.Zero };
zeros.Options.Add(s0);
zeros.Options.Add(i0);
root.Subcommands.Add(zeros);

// =====================================================================

Section("A. Default arity per option type");
Arity("Option<bool>", new Option<bool>("--x"));
Arity("Option<bool?>", new Option<bool?>("--x"));
Arity("Option<string>", new Option<string>("--x"));
Arity("Option<int>", new Option<int>("--x"));
Arity("Option<Color>", new Option<Color>("--x"));
Arity("Option<Color?>", new Option<Color?>("--x"));
Arity("Option<Color[]>", new Option<Color[]>("--x"));
Arity("Option<string[]>", new Option<string[]>("--x"));

Section("B. Help for enum options");
root.Parse("enums -h").Invoke();

Section("C. Completions offered after each enum option");
foreach(string opt in new[] { "--color", "--color-n", "--colors", "--perms", "--color-s", "--colors-s" }) {
	string line = $"enums {opt} ";
	string[] items = [.. root.Parse(line).GetCompletions(line.Length).Select(c => c.Label)];
	WriteLine($"{opt,-12} -> {string.Join(" ", items)}");
}

string argLine = "enums ";
string[] argItems = [.. root.Parse(argLine).GetCompletions(argLine.Length).Select(c => c.Label).Where(l => !l.StartsWith('-'))];
WriteLine($"{"<shade> arg",-12} -> {string.Join(" ", argItems)}");

Section("D. Enum parsing");
Show("enums --color red", r => r.GetValue(color));
Show("enums --color BLUE", r => r.GetValue(color));
Show("enums --color 2", r => r.GetValue(color));
Show("enums --color 99", r => r.GetValue(color));
Show("enums --color Red,Blue", r => r.GetValue(color));
Show("enums --color \" green \"", r => r.GetValue(color));
Show("enums --color bogus", r => r.GetValue(color));
Show("enums", r => $"color={r.GetValue(color)} color-n={Fmt(r.GetValue(colorN))} colors={Fmt(r.GetValue(colors))}");
Show("enums --color-n green", r => Fmt(r.GetValue(colorN)));
Show("enums --perms read,write", r => r.GetValue(perms));
Show("enums --perms 3", r => r.GetValue(perms));
Show("enums --perms Read|Write", r => r.GetValue(perms));
Show("enums --colors red blue", r => Fmt(r.GetValue(colors)));
Show("enums --colors red --colors blue", r => Fmt(r.GetValue(colors)));
Show("enums --colors red,blue", r => Fmt(r.GetValue(colors)));
Show("enums --colors red bogus", r => Fmt(r.GetValue(colors)));
Show("enums blue", r => r.GetValue(colorArg));
Show("enums bogus", r => r.GetValue(colorArg));

Section("E. Bool flags");
Show("flags s1 --apply false", r => $"apply={r.GetValue(apply)} name={r.GetValue(name)}");
Show("flags s1 --apply", r => $"apply={r.GetValue(apply)} name={r.GetValue(name)}");
Show("flags --apply s1", r => $"apply={r.GetValue(apply)} name={r.GetValue(name)}");
Show("flags s1 --apply=false", r => $"apply={r.GetValue(apply)} name={r.GetValue(name)}");
Show("flags s1 --flag0", r => $"flag0={r.GetValue(flag0)} name={r.GetValue(name)}");
Show("flags --flag0 s1", r => $"flag0={r.GetValue(flag0)} name={r.GetValue(name)}");
Show("flags s1 --flag0 false", r => $"flag0={r.GetValue(flag0)} name={r.GetValue(name)}");
Show("flags s1 --flag0=false", r => $"flag0={r.GetValue(flag0)} name={r.GetValue(name)}");
Show("flags s1", r => $"apply={r.GetValue(apply)} flag0={r.GetValue(flag0)} apply-n={Fmt(r.GetValue(applyN))} flag0-n={Fmt(r.GetValue(flag0N))}");
Show("flags s1 --apply-n --flag0-n", r => $"apply-n={Fmt(r.GetValue(applyN))} flag0-n={Fmt(r.GetValue(flag0N))}");
Show("flags-rest --flag0 false x", r => $"flag0={r.GetValue(flag0R)} rest=[{string.Join(", ", r.GetValue(rest) ?? [])}]");

Section("F. Help for bool flags");
root.Parse("flags -h").Invoke();

Section("G. Zero arity on non-bool options");
Show("zeros --s0", r => $"s0={Fmt(r.GetValue(s0))}");
Show("zeros --i0", r => $"i0={r.GetValue(i0)}");
Show("zeros --s0 x", r => $"s0={Fmt(r.GetValue(s0))}");

// =====================================================================

void Section(string title)
{
	WriteLine();
	WriteLine($"===== {title} =====");
}

void Arity(string label, Option o)
	=> WriteLine($"{label,-18} min={o.Arity.MinimumNumberOfValues} max={o.Arity.MaximumNumberOfValues}");

void Show(string cmdln, Func<ParseResult, object> get)
{
	ParseResult r = root.Parse(cmdln);
	if(r.Errors.Count > 0) {
		WriteLine($"[{cmdln}] -> ERRORS:");
		foreach(ParseError e in r.Errors)
			WriteLine($"    {e.Message.Replace("\n", "\n    ")}");
		return;
	}

	WriteLine($"[{cmdln}] -> {get(r)}");
}

static string Fmt(object v) => v switch {
	null => "null",
	string s => $"\"{s}\"",
	Array a => $"{a.GetType().Name}[{string.Join(", ", a.Cast<object>())}]",
	_ => v.ToString(),
};

enum Color { Red, Green, Blue }

[Flags]
enum Perms { None = 0, Read = 1, Write = 2, Exec = 4 }

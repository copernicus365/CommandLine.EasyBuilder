using System.CommandLine;

using EasyBuilder.Samples.GetStarted;
using EasyBuilder.Samples.Test1;

using static System.Console;

namespace EasyBuilder.Samples;

class Program
{
	public static async Task<int> Main(string[] args)
	{
		RootCommand rootCmd = GetApp(args);
		string cmdln = args.IsNulle() ? "-h" : args[0];

		if(args.IsNulle())
			cmdln = "-h";

		do {
			if(cmdln.IsNulle()) {
				Write(">> ");
				cmdln = ReadLine();
			}

			ParseResult res = rootCmd.Parse(cmdln);
			cmdln = null;

			if(res.Errors.Any()) {
				foreach(var err in res.Errors)
					WriteLine($"Error: {err.Message}");
				WriteLine();
				continue;
			}

			int dRes = await res.InvokeAsync();
		} while(true);
	}

	public static RootCommand GetApp(string[] args)
	{
		int progNum = 7;
		RootCommand root = progNum switch {
			4 => new ExampleApp4_Auto().GetApp(),
			5 => new ExampleApp5_Auto().GetApp(),
			7 => ExampleApp7.GetApp(),
			10 => new GetStartedTutorial().Build(),
			11 => new GetStartedTutorialSimple().BuildEasy(),
			_ => throw new ArgumentOutOfRangeException(),
		};
		return root;
	}
}

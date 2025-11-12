using System.CommandLine;

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
			WriteLine();
		} while(true);
	}

	public static RootCommand GetApp(string[] args)
	{
		int progNum = 1;
		RootCommand root = progNum switch {
			1 => ExampleApp_HelloWorld.GetApp(),
			2 => new A.ExampleApp_ReadCmd().GetApp(),
			3 => new ExampleApp_Person().GetApp(),
			4 => new ExampleApp_Fun().GetApp(),
			5 => new GetStartedTutorial_Original().Build(),
			6 => new GetStartedTutorial_Auto().GetApp(),
			_ => throw new ArgumentOutOfRangeException(),
		};
		return root;
	}
}

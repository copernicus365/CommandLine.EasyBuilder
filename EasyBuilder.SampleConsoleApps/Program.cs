using System.CommandLine;

using EasyBuilder.Samples.GetStarted;
using EasyBuilder.Samples.Test1;

using static System.Console;

namespace EasyBuilder.Samples;

class Program
{
	public static async Task<int> Main(string[] args)
	{
		bool getNew = true;
		int progNum = 11;

		string appDescription = ExampleAppDescription(progNum, getNew);

		RootCommand root = progNum switch {
			//1 => new ExampleApp1(appDescription).GetApp(getNew),
			//2 => new ExampleApp2(appDescription).GetApp(getNew),
			//3 => new ExampleApp3(appDescription).GetApp(getNew),
			4 => new ExampleApp4_Auto(appDescription).GetApp(),
			5 => new ExampleApp5_Auto().GetApp(),
			6 => new ExampleApp6_Simple().GetApp(),
			10 => new GetStartedTutorial().Build(),
			11 => new GetStartedTutorialSimple().BuildEasy(), // .Build(),
			_ => throw new ArgumentOutOfRangeException(),
		};

		if(args.NotNulle()) {
			ParseResult res = root.Parse(args);
			int dRes = await res.InvokeAsync();
			return dRes;
		}

		bool showHelp = true;
		if(showHelp) {
			//root.Invoke("-h");
		}

		while(true) {
			Write("Input: ");

			string cmdLine = ReadLine();
			WriteLine();

			ParseResult res = root.Parse(cmdLine);
			int dRes = await res.InvokeAsync();

			WriteLine();
			WriteLine();
		}

		return 0;
	}

	public static string ExampleAppDescription(int num, bool isNew)
		=> $"Example App {num} ({(isNew ? "new" : "old")}): compare orig command line builder vs EasyBuilder's functional approach.\nExample code adapted from tutorials @ https://learn.microsoft.com/en-us/dotnet/standard/commandline/get-started-tutorial";
}

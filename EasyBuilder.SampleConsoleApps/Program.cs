using System.CommandLine;

using EasyBuilder.Samples.Test1;

using static System.Console;

namespace EasyBuilder.Samples;

class Program
{
	public static async Task<int> Main(string[] args)
	{
		bool getNew = true;
		int progNum = 5;

		string appDescription = ExampleAppDescription(progNum, getNew);

		RootCommand rootCommand = progNum switch {
			1 => new ExampleApp1(appDescription).GetApp(getNew),
			2 => new ExampleApp2(appDescription).GetApp(getNew),
			3 => new ExampleApp3(appDescription).GetApp(getNew),
			4 => new ExampleApp4_Auto(appDescription).GetApp(),
			5 => new ExampleApp5_Auto().GetApp(),
			_ => throw new ArgumentOutOfRangeException(),
		};

		rootCommand.Invoke("-h");

		while(true) {
			Write("Input: ");
			string arg = ReadLine();

			int res = arg == null
				? await rootCommand.InvokeAsync(args)
				: await rootCommand.InvokeAsync(arg);
		}
	}

	public static string ExampleAppDescription(int num, bool isNew)
		=> $"Example App {num} ({(isNew ? "new" : "old")}): compare orig command line builder vs EasyBuilder's functional approach.\nExample code adapted from tutorials @ https://learn.microsoft.com/en-us/dotnet/standard/commandline/get-started-tutorial";
}

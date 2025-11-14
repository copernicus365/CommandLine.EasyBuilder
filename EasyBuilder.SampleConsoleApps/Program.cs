using System.CommandLine;

using CommandLine.EasyBuilder;

using static System.Console;

namespace EasyBuilder.Samples;

class Program
{
	public static async Task<int> Main(string[] args)
	{
		SampleAppKind kind = SampleAppKind.HelloWorld;

		RootCommand rootCmd = GetSampleApp(kind, args);
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

	static RootCommand GetSampleApp(SampleAppKind kind, string[] args)
	{
		RootCommand root = kind switch {
			SampleAppKind.ReadCmd => ReadFileApp.Build(),
			SampleAppKind.Fun => FunCmdApp.Build(),
			SampleAppKind.GetStarted_Auto => GetStartedTutorialApp.Build(),
			SampleAppKind.GetStarted_Orig => new GetStartedTutorialApp_Original().Build(),
			_ => null,
		};

		if(root is not null)
			return root;

		root = new("Command line is cool");

		switch(kind) {
			case SampleAppKind.HelloWorld:
				root.AddAutoCommand<HellowWorldCmd>(); break;
			case SampleAppKind.Person:
				root.AddAutoCommand<PersonCmd>(); break;
			default:
				throw new ArgumentOutOfRangeException();
		}
		return root;
	}

	enum SampleAppKind
	{
		HelloWorld = 1,
		ReadCmd = 2,
		Person = 3,
		Fun = 4,
		GetStarted_Orig = 5,
		GetStarted_Auto = 6,
	};
}

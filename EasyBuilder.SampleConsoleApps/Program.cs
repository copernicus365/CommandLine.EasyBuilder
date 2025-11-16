using System.CommandLine;

using CommandLine.EasyBuilder;

using static System.Console;

namespace EasyBuilder.Samples;

class Program
{
	public static async Task<int> Main(string[] args)
	{
		string cmdln = args.IsNulle() ? "-h" : args[0]; // simplifies for demo to single cmd line arg

		RootCommand rootCmd = null;
		do {
			if(rootCmd == null || ResetKind) {
				rootCmd = GetSampleApp(Kind, args);
				cmdln ??= "-h";
				ResetKind = false;
			}

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

		if(root is null) {
			root = new("Command line is cool");

			switch(kind) {
				case SampleAppKind.HelloWorld:
					root.AddAutoCommand<HelloWorldCmd>(); break;
				case SampleAppKind.NumberArrays:
					root.AddAutoCommand<NumberArrayCmd>(); break;
				case SampleAppKind.Person:
					root.AddAutoCommand<PersonCmd>(); break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		if(AllowChangeDemo)
			root.AddAutoCommand<ChangeDemoAppCLIKindCmd>();
		return root;
	}

	public static SampleAppKind Kind = SampleAppKind.NumberArrays; //.HelloWorld;
	public static bool ResetKind = false;
	public static bool AllowChangeDemo = true;
}

public enum SampleAppKind
{
	HelloWorld,
	ReadCmd,
	Person,
	Fun,
	NumberArrays,
	GetStarted_Orig,
	GetStarted_Auto,
};

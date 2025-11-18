using System.CommandLine;

using CommandLine.EasyBuilder;

using EasyBuilder.Samples.Tutorial;

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
			SampleAppKind.Fun => FunCmdApp.Build(),
			SampleAppKind.ReadFile => ReadFileApp.Build(),
			SampleAppKind.GetStarted => GetStartedTutorialApp.Build(),
			SampleAppKind.GetStartedOrig => new GetStartedTutorialApp_Original().Build(),
			_ => null,
		};

		if(root is null) {
			root = new("Command line is cool");

			switch(kind) {
				case SampleAppKind.Hello:
					root.AddAutoCommand<HelloWorldCmd>(); break;
				case SampleAppKind.NumArrays:
					root.AddAutoCommand<NumberArrayCmd>(); break;
				case SampleAppKind.Person:
					root.AddAutoCommand<PersonCmd>(); break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		if(AllowChangeDemo)
			root.AddAutoCommand<DemoAppCmd>();
		return root;
	}

	public static SampleAppKind Kind = SampleAppKind.NumArrays; //.HelloWorld;
	public static bool ResetKind = false;
	public static bool AllowChangeDemo = true;
}

public enum SampleAppKind
{
	Hello,
	ReadFile,
	Person,
	Fun,
	NumArrays,
	GetStarted,
	GetStartedOrig,
};

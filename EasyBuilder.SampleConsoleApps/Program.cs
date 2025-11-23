global using static System.Console;

using System.CommandLine;

using CommandLine.EasyBuilder;

using EasyBuilder.Samples.Tutorial;

namespace EasyBuilder.Samples;

class Program
{
	public static SampleAppKind Kind = SampleAppKind.AutoDefaultsFun; //.HelloWorld;
	static bool RunSingleSimpleHelloWorldApp = false;

	/// <summary>
	/// To instead run `Program_HostedApp_WithDI.cs` minimal / top-level hosted app,
	/// `Main` must be renamed (temporarily or whatever).
	/// </summary>
	public static async Task Main(string[] args)
	{
		// since this file has got long for demo purposes, for simplest case see:
		if(RunSingleSimpleHelloWorldApp) {
			await HelloWorldApp.Main_Run(args);
			return;
		}

		string cmdln = args.IsNulle() ? "-h" : args[0]; // simplifies for demo to single cmd line arg

		RootCommand rootCmd = null;
		do {
			if(rootCmd == null || ResetKind) {
				rootCmd = GetSampleApp(Kind);
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

	static RootCommand GetSampleApp(SampleAppKind kind)
	{
		RootCommand root = kind switch {
			SampleAppKind.Fun => FunCmdApp.Build(),
			SampleAppKind.ReadFile => ReadFileApp.Build(),
			SampleAppKind.GetStarted => GetStartedTutorialApp.Build(),
			SampleAppKind.GetStartedOrig => new GetStartedTutorialApp_Original().Build(),
			_ => null,
		};

		if(root is null) {
			if(SampleAppsSetAsRoot) {
				root = new("Command line is cool");
				switch(kind) {
					case SampleAppKind.Hello: root.SetAutoCommand<HelloWorldCmd>(); break;
					case SampleAppKind.NumArrays: root.SetAutoCommand<NumberArrayCmd>(); break;
					case SampleAppKind.Person: root.SetAutoCommand<PersonCmd>(); break;
					case SampleAppKind.AutoDefaultsFun: root.SetAutoCommand<AutoDefaultsFunCmd>(); break;
					default: throw new ArgumentOutOfRangeException();
				}
			}
			else {
				root = new("Command line is cool");
				switch(kind) {
					case SampleAppKind.Hello: root.AddAutoCommand<HelloWorldCmd>(); break;
					case SampleAppKind.NumArrays: root.AddAutoCommand<NumberArrayCmd>(); break;
					case SampleAppKind.Person: root.AddAutoCommand<PersonCmd>(); break;
					case SampleAppKind.AutoDefaultsFun: root.AddAutoCommand<AutoDefaultsFunCmd>(); break;
					default: throw new ArgumentOutOfRangeException();
				}
			}
		}

		if(AllowChangeDemo)
			root.AddAutoCommand<DemoAppCmd>();
		return root;
	}

	public static bool ResetKind = false;
	public static bool SampleAppsSetAsRoot = false;
	public static bool AllowChangeDemo = true;
}

public enum SampleAppKind
{
	Hello,
	ReadFile,
	Person,
	AutoDefaultsFun,
	Fun,
	NumArrays,
	GetStarted,
	GetStartedOrig,
};

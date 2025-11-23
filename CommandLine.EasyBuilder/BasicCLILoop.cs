using System.CommandLine;

using static System.Console;

namespace CommandLine.EasyBuilder;

/// <summary>Simple command line loop runner.</summary>
/// <remarks>
/// For full-fledged apps you'd write your own, sure. But there are many simple use cases,
/// especially single-file apps, where removing 30 extra lines of code is very welcomed.
/// </remarks>
public class BasicCLILoop
{
	/// <summary>Simple full demonstration</summary>
	public static async Task Run(RootCommand rootCmd, string[] args, bool doLoop = false, string noArgsDefaultCmd = "-h")
	{
		string cmdln = (args == null || args.Length == 0) ? noArgsDefaultCmd : args[0]?.Trim();
		bool loopAgain = false;
		do {
			if(string.IsNullOrWhiteSpace(cmdln)) {
				Write(">> ");
				cmdln = ReadLine()?.Trim();
			}

			ParseResult res = rootCmd.Parse(cmdln);
			loopAgain = cmdln == "-h" || cmdln == "--help"; // string.IsNullOrEmpty(cmdln)
			cmdln = null;

			if(res.Errors.Any()) {
				foreach(var err in res.Errors)
					WriteLine($"Error: {err.Message}");
				WriteLine();
				continue;
			}

			await res.InvokeAsync();
			WriteLine();
		} while(doLoop || loopAgain);
	}
}

using System.CommandLine;

namespace EasyBuilder.Samples;

public class SampleCLILoopRunner
{
	/// <summary>Simple full demonstration</summary>
	public static async Task Run(RootCommand rootCmd, string[] args = null)
	{
		string cmdln = args.IsNulle() ? "-h" : args[0];
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

			await res.InvokeAsync();
			WriteLine();
		} while(true);
	}
}

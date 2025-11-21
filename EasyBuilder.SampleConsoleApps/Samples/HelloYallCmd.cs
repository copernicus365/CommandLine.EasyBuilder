using System.CommandLine;

using CommandLine.EasyBuilder;

using Microsoft.Extensions.Logging;

namespace EasyBuilder.Samples.WithServices;

[Command("hello", "Hello commandline world!")]
public class HelloYallCmd(ICoolSvc coolSvc)
{
	[Option("--name", "-n", required: true)]
	public string Name { get; set; }

	[Option("--age")]
	public int Age { get; set; } = 12;

	public void Handle()
		=> coolSvc.Write($"Hello {Name} ({Age})!");
}

[Command("mello", "Example cmd with no service (to show both work at same time, ones with DI and ones w/out)")]
public class MellowCmd
{
	[Option("--name", "-n", required: true)]
	public string Name { get; set; }

	public void Handle()
		=> WriteLine($"MELLOW {Name.ToLower()}!");
}

public class HelloYallCmdApp
{
	public static RootCommand GetApp()
	{
		RootCommand rootCmd = new("Command line is cool");
		rootCmd.AddAutoCommand<HelloYallCmd>();
		rootCmd.AddAutoCommand<MellowCmd>();
		return rootCmd;
	}

	/// <summary>Simple full demonstration</summary>
	public static async Task Run(string[] args = null)
	{
		RootCommand rootCmd = GetApp();

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

public class HelloYall(ICoolSvc coolSvc)
{
	public void Doda(string msg = "howdy") => coolSvc.Write(msg);
}

public interface ICoolSvc { void Write(string message); }

public class CoolSvc(ILogger<CoolSvc> logger) : ICoolSvc
{
	public void Write(string message)
	{
		logger.LogInformation("COOL log: {Message}!", message);
		WriteLine($"COOL BeaNS says: {message}!");
	}
}

using System.CommandLine;

using CommandLine.EasyBuilder;

using Microsoft.Extensions.Logging;

namespace EasyBuilder.Samples;

[Command("hello", "Hello DI commandline world!")]
public class HelloDICmd(ICoolSvc coolSvc)
{
	[Option("--name", "-n", required: true)]
	public string Name { get; set; }

	[Option("--age")]
	public int Age { get; set; } = 12;

	public void Handle() => coolSvc.Write($"Hello {Name} ({Age})!");
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

[Command("mello", "Example cmd with no service (to show both work at same time, ones with DI and ones w/out)")]
public class MellowCmd
{
	[Option("--name", "-n", required: true)]
	public string Name { get; set; }

	public void Handle() => WriteLine($"MELLOW {Name.ToLower()}!");
}

public class HelloDIApp
{
	public static RootCommand GetApp()
	{
		RootCommand rootCmd = new("Command line is cool");
		rootCmd.AddAutoCommand<HelloDICmd>();
		rootCmd.AddAutoCommand<MellowCmd>();
		return rootCmd;
	}
}

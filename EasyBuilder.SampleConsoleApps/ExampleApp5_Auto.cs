using System.CommandLine;

using CommandLine.EasyBuilder;
using CommandLine.EasyBuilder.Auto;

namespace EasyBuilder.Samples.Test1;

public class ExampleApp5_Auto
{
	public RootCommand GetApp()
	{
		RootCommand rootCmd = new("Testing default and nullable type issues...");

		Command teachCmd = new("teach", "Fun foo bar command");
		rootCmd.AddCommand(teachCmd);

		teachCmd.AddAutoCommand<FunArgs>();
			//.AddAutoCommand((FunArgs v) => v.Handle());

		return rootCmd;
	}
}

public enum FunnyType { None = 0, Dry = 1, Crackup = 2 }

[Command("fun", "Silly test type for dealing with default and nullable value types")]
public class FunArgs
{
	[Argument("arg1", description: "Testing?!")]
	public string Arg1 { get; set; }

	[Option("--name", "-n", DefVal = "Charlie Brown")]
	public string Name { get; set; }

	[Option("--funny", DefVal = FunnyType.Dry, Description = "Foreground color of text displayed on the console")] //DefVal = ConsoleColor.White,
	public FunnyType? Fun { get; set; }

	[Option("--delay", "-d", DefVal = 42, Description = "Delay between lines, specified as milliseconds per character in a line")]
	public double? Delay { get; set; }

	public void Handle()
		=> $"Hello {Name}, you are {Fun} funny type, with {Delay} delay. And oh, arg = `{Arg1?.ToUpper()}`".Print();
}

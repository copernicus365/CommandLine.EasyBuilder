using System.CommandLine;

using CommandLine.EasyBuilder;
using CommandLine.EasyBuilder.Auto;

namespace EasyBuilder.Samples.Test1;

public class ExampleApp5_Auto
{
	public RootCommand GetApp()
	{
		RootCommand rootCmd = new("Cool app!");

		Command teachCmd = new("teach", "Ben Stein says teach this well");
		rootCmd.AddCommand(teachCmd);

		teachCmd.AddAutoCommand<FunArgs>();

		return rootCmd;
	}
}

[Command("fun", "Silly test type for dealing with default and nullable value types")]
public class FunArgs
{
	[Option(
		"--last-name",
		"-ln",
		description: "Bogus last name",
		DefVal = "Fergy"
		//Required = true
		)]
		public string LastName { get; set; }

	[Argument("arg", description: "I'm an argument")]
	public string Arg1 { get; set; }

	[Option("--name", "-n", DefVal = "Charlie Brown")]
	public string Name { get; set; }

	[Option("--funny", DefVal = FunnyType.Dry, Description = "Foreground color of text displayed on the console")] //DefVal = ConsoleColor.White,
	public FunnyType? Fun { get; set; }

	[Option("--delay", "-d", DefVal = 42, Description = "Delay between lines, specified as milliseconds per character in a line")]
	public double? Delay { get; set; }

	public void Handle()
		=> $"Hello {Name} {LastName}, you are {Fun} funny type, with {Delay} delay. And oh, arg = `{Arg1?.ToUpper()}`".Print();
}

public enum FunnyType { None = 0, Dry = 1, Crackup = 2 }

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
		rootCmd.Subcommands.Add(teachCmd);

		teachCmd.AddAutoCommand<FunArgs>();

		return rootCmd;
	}
}

[Command("fun", "Silly test type for dealing with default and nullable value types")]
public class FunArgs
{
	[Option("--name", "-n", DefVal = "Charlie Brown")]
	public string Name { get; set; }

	[Option(name: "--last-name", alias: "-ln", description: "Bogus last name", DefVal = "Fergy", Required = true)]
	public string LastName { get; set; }

	//[Argument("arg", description: "I'm an argument")]
	//public string Arg1 { get; set; }

	//[Option("--funny", DefVal = FunnyType.Dry, Description = "Foreground color of text displayed on the console")] //DefVal = ConsoleColor.White,
	//public FunnyType? Fun { get; set; }

	[Option("--delay", "-d", DefVal = 42, Description = "Delay between lines, specified as milliseconds per character in a line")]
	public int? Delay { get; set; }

	public void Handle(ParseResult result)
		=> "hi!".Print();

	public async Task Handle2(ParseResult result)
		=> "hi async!".Print();
}

public enum FunnyType { None = 0, Dry = 1, Crackup = 2 }





// ---

//[Command("funny", "Silly test type for dealing with default and nullable value types")]

public class FunnyArgs
{
	public void Handle(ParseResult result) => "hi!".Print();
}
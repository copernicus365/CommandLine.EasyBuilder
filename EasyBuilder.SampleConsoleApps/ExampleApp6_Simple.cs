using System.CommandLine;

using CommandLine.EasyBuilder;

namespace EasyBuilder.Samples.Test1;

public class ExampleApp6_Simple
{
	public RootCommand GetApp()
	{
		RootCommand rootCmd = new("Cool app!");

		Command teachCmd = new("teach", "Ben Stein says teach this well");
		rootCmd.Subcommands.Add(teachCmd);
		rootCmd.AddAutoCommand<HellowWorld>();

		return rootCmd;
	}
}

[Command("hi", "Fun test command")]
public class HellowWorld
{
	[Option("--name", "-n", Required = true)]
	public string Name { get; set; }

	[Option(name: "--age", Required = true)]
	public int Age { get; set; }

	[Option("--delay", "-d", DefVal = 4, Description = "Print delay in seconds")]
	public int? Delay { get; set; }

	[Option("--animal", "-a", Required = true)]
	public FavoriteAnimal FavAnimal { get; set; } = FavoriteAnimal.Cheetah;

	public ParseResult ParseResult { get; set; }

	public async Task HandleAsync()
	{
		if(Delay > 0)
			await Task.Delay(TimeSpan.FromSeconds(Delay.Value));
		PrintIt();
	}

	public void PrintIt()
		=> $"Hi {Name} ({Age}), here's a {FavAnimal}!".Print();
}

public enum FavoriteAnimal { None = 0, Dog = 1, Cat = 2, Cheetah = 3, Rhino = 4 }

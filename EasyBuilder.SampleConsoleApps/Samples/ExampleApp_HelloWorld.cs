using System.CommandLine;

using CommandLine.EasyBuilder;

using static System.Console;

namespace EasyBuilder.Samples;

public class ExampleApp_HelloWorld
{
	public static RootCommand GetApp()
	{
		RootCommand rootCmd = new("Command line is cool");
		rootCmd.AddAutoCommand<HellowWorld>();
		return rootCmd;
	}
}

[Command("hello", "Hello commandline world!")]
public class HellowWorld
{
	[Option("--name", "-n", Required = true)]
	public string Name { get; set; }

	[Option(name: "--age", DefVal = 42)]
	public int Age { get; set; }

	[Option("--animal", "-a", Required = true)]
	public FavoriteAnimal FavAnimal { get; set; } = FavoriteAnimal.Cheetah;

	public void Handle()
		=> WriteLine($"Hello {Name} ({Age}), glad to see you love {FavAnimal}s!");
}

public enum FavoriteAnimal { None = 0, Dog = 1, Cat = 2, Cheetah = 3, Rhino = 4 }

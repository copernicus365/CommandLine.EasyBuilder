using System.CommandLine;

using CommandLine.EasyBuilder;

namespace EasyBuilder.Samples;

[Command("hello", "Hello commandline world!")]
public class HelloWorldCmd
{
	[Option("--name", "-n", required: true)]
	public string Name { get; set; }

	[Option("--age")]
	public int Age { get; set; }

	[Option("--animal", "-a", required: true)]
	public FavoriteAnimal FavAnimal { get; set; } = FavoriteAnimal.Cheetah;

	public void Handle()
		=> WriteLine($"Hello {Name} ({Age}), glad to see you love {FavAnimal}s!");
}

public enum FavoriteAnimal { None = 0, Dog = 1, Cat = 2, Cheetah = 3, Rhino = 4 }

public class HelloWorldApp
{
	// rename `Main_Run` to `Main` to run as a standalone app
	public static async Task Main_Run(string[] args)
		=> await BasicCLILoop.Run(GetApp(false), args, doLoop: true);

	public static RootCommand GetApp(bool makeSubcommand)
	{
		RootCommand rootCmd = new("Command line is cool");
		if(makeSubcommand)
			rootCmd.AddAutoCommand<HelloWorldCmd>();
		else
			rootCmd.SetAutoCommand<HelloWorldCmd>();
		return rootCmd;
	}
}

using System.CommandLine;

using CommandLine.EasyBuilder;

namespace EasyBuilder.Samples;

[Command("hello", "Hello commandline world!")]
public class HelloWorldCmd
{
	[Option("--name", "-n", required: true)]
	public string Name { get; set; }

	[Option("--age")] //, DefVal = 42)]
	public int Age { get; set; } = 12;

	[Option("--animal", "-a", required: true)]
	public FavoriteAnimal FavAnimal { get; set; } = FavoriteAnimal.Cheetah;

	public void Handle()
		=> WriteLine($"Hello {Name} ({Age}), glad to see you love {FavAnimal}s!");
}

public enum FavoriteAnimal { None = 0, Dog = 1, Cat = 2, Cheetah = 3, Rhino = 4 }

public class HelloWorldApp
{
	// rename `Main_Run` to `Main` to run as a standalone app
	public static async Task<int> Main_Run(string[] args)
	{
		RootCommand root = GetApp(false);
		await BasicCLILoop.Run(root, args, doLoop: true);
		return 0;
	}

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

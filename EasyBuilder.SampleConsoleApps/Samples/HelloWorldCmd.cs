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
	public static RootCommand GetApp(bool makeSubcommand)
	{
		RootCommand rootCmd = new("Command line is cool");
		if(makeSubcommand)
			rootCmd.AddAutoCommand<HelloWorldCmd>();
		else
			rootCmd.SetAutoCommand<HelloWorldCmd>();
		return rootCmd;
	}

	/// <summary>Simple full demonstration</summary>
	public static async Task Run(string[] args = null)
	{
		RootCommand rootCmd = GetApp(makeSubcommand: false);

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

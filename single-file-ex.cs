#:package CommandLine.EasyBuilder@2.0.0-rc1.7

using static System.Console;
using System.CommandLine;
using CommandLine.EasyBuilder;

WriteLine("Single file .cs app CLI example (let's GO csharp!)");

RootCommand rootCmd = new("Command line is cool");
rootCmd.AddAutoCommand<HelloWorldCmd>();

await BasicCLILoop.Run(rootCmd, args, doLoop: true);


[Command("hello", "Hello commandline world!")]
public class HelloWorldCmd
{
	[Option("--name", "-n", required: true)]
	public string Name { get; set; }

	[Option("--age", required: true)]
	public int Age { get; set; }

	[Option("--animal", "-a", required: true)]
	public FavoriteAnimal FavAnimal { get; set; } = FavoriteAnimal.Cheetah;

	public void Handle()
		=> WriteLine($"Hello {Name} ({Age}), glad to see you love {FavAnimal}s!");
}

public enum FavoriteAnimal { None, Dog, Cat, Cheetah, Rhino }

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
		=> Console.WriteLine($"Hello {Name} ({Age}), glad to see you love {FavAnimal}s!");
}

public enum FavoriteAnimal { None = 0, Dog = 1, Cat = 2, Cheetah = 3, Rhino = 4 }

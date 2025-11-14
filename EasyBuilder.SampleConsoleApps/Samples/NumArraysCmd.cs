using CommandLine.EasyBuilder;

namespace EasyBuilder.Samples;

[Command("hello", "Hello commandline world!")]
public class NumArraysCmd
{
	[Option("--name", "-n", Required = true)]
	public string Name { get; set; }

	[Option(name: "--age", DefVal = 42)]
	public int Age { get; set; }

	[Option("--animal", "-a", Required = true)]
	public FavoriteAnimal FavAnimal { get; set; } = FavoriteAnimal.Cheetah;

	public void Handle()
		=> Console.WriteLine($"Hello {Name} ({Age}), glad to see you love {FavAnimal}s!");
}

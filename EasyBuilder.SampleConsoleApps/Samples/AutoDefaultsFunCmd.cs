using System.CommandLine;

using CommandLine.EasyBuilder;

namespace EasyBuilder.Samples;

[Command("autofun", "Demoing auto-default value setting fun")]
public class AutoDefaultsFunCmd
{
	[Option("--name", "-n", required: true)]
	public string Name { get; set; }

	[Option("--age")]
	public int Age { get; set; }

	[Option("--age1", DefVal = 24)]
	public int Age1 { get; set; }

	[Option("--age2")]
	public int Age2 { get; set; } = 42;

	[Option("--animal", "-a", required: true)]
	public FavoriteAnimal FavAnimal { get; set; } = FavoriteAnimal.Cheetah;

	public void Handle()
		=> WriteLine($"Hello {Name}! From {Age} to {Age1} and even to {Age2}, y'all still love {FavAnimal}s");
}

using System.CommandLine;

using CommandLine.EasyBuilder;

namespace EasyBuilder.Samples;

[Command("person", "Coolio person...")]
public class PersonCmd
{
	[Option("--first-name", "-fn", Required = true, Description = "Person's first name")]
	public string FirstName { get; set; }

	[Option("--last-name", "-ln", Required = true)]
	public string LastName { get; set; }

	[Option("--age", Description = "Person's age", DefVal = 55)]
	public int Age { get; set; }

	[Option("--fav-number", Description = "Person's age")]
	public int FavNumber { get; set; } = 7;

	[Option("--nada-n", Description = "Ignore...")]
	public double NadaN { get; set; }

	[Option("--durations", "-durs", Description = "Person's age", Required = true)]
	public string DurationsArg {
		get => Durations.JoinToString(",");
		set {
			if(!ArgParser.TryParseNumberArray<double>(value, out var durs))
				throw new ArgumentException("Invalid array");
			Durations = durs; // ArgParser.TryParseNumberArray( .DoubleArray(value, out string err);
		}
	}

	public double[] Durations { get; set; }

	public void Handle()
		=> Console.WriteLine($"Person: {FirstName} {LastName}, Age: {Age}, FavNum: {FavNumber}, Durs: {Durations?.JoinToString(",")}");
}

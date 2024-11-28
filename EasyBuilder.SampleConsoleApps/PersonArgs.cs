using CommandLine.EasyBuilder;
using CommandLine.EasyBuilder.Auto;

namespace EasyBuilder.Samples.Test1;

[Command("dude", "Coolio a person man...")]
public class PersonArgs
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
		set => Durations = ArgParsers.DoubleArray(value, out string err);
	}

	public double[] Durations { get; set; }
}

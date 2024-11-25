using CommandLine.EasyBuilder;
using CommandLine.EasyBuilder.Auto;

namespace EasyBuilder.Samples.Test1;

[Control(Name = "dude", Alias = "p", Description = "Coolio a person man...")]
public class Duder : IControl
{
	[Option(Name = "--first-name", Alias = "-fn", Required = true, Description = "Person's first name")]
	public string FirstName { get; set; }

	[Option(Name = "--last-name", Alias = "-ln", Required = true)]
	public string LastName { get; set; }

	[Option(Name = "--age", Description = "Person's age", DefVal = 55)]
	public int Age { get; set; }

	[Option(Name = "--fav-number", Description = "Person's age")]
	public int FavNumber { get; set; } = 7;

	[Option(Name = "--nada-n", Description = "Ignore...")]
	public double NadaN { get; set; }


	[Option(Name = "--durations", Alias = "-durs", Description = "Person's age", Required = true)]
	public string DurationsArg {
		get => Durations.JoinToString(",");
		set => Durations = ArgParsers.DoubleArray(value, out string err);
	}

	public double[] Durations { get; set; }
}

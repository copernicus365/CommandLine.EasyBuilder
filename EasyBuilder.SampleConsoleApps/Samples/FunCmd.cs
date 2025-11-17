using System.CommandLine;

using CommandLine.EasyBuilder;

namespace EasyBuilder.Samples;

public class FunCmdApp
{
	public static RootCommand Build()
	{
		RootCommand rootCmd = new("Cool app!");

		Command teachCmd = new("teach", "Ben Stein says teach this well");
		rootCmd.Subcommands.Add(teachCmd);

		teachCmd.AddAutoCommand<FunCmd>();

		return rootCmd;
	}
}

[Command("fun", "Silly test type for dealing with default and nullable value types")]
public class FunCmd
{
	[Option("--name", "-n", DefVal = "Charlie")]
	public string Name { get; set; }

	[Option("--last-name", "-ln", required: true, description: "Bogus last name")]
	public string LastName { get; set; }

	[Option("--pw", "-p", required: true)]
	public string Pword { get; set; }

	[Option("--age", DefVal = 47)]
	public int Age { get; set; }

	[Option("--fav-num")]
	public int? FavoriteNumber { get; set; }

	[Option("--delay", "-d", DefVal = 4, Description = "Delay between lines, specified as milliseconds per character in a line")]
	public int? Delay { get; set; }

	[Option("--fun", "-f", required: true)]
	public FunnyType Fun { get; set; } // = FunnyType.Dry;

	public ParseResult ParseResult { get; set; }

	public void Handle1() => PrintIt();

	public async Task HandleAsync()
	{
		if(Delay > 0)
			await Task.Delay(TimeSpan.FromSeconds(Delay.Value));
		PrintIt();
	}

	public void PrintIt()
		=> $"hi {Name} {LastName} {Fun} ({Age}) cool num {FavoriteNumber} (delay: {Delay})!".Print();
}

public enum FunnyType { None = 0, Dry = 1, Crackup = 2 }

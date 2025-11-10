using System.CommandLine;

using CommandLine.EasyBuilder;
using CommandLine.EasyBuilder.Auto;

using static System.Console;

namespace EasyBuilder.Samples.GetStarted;

public class GetStartedTutorialSimple
{
	public RootCommand BuildEasy()
	{
		RootCommand rootCmd = new("Sample app for System.CommandLine");

		Command quotesCmd = rootCmd.AddAutoCommand<QuotesCmd>();

		quotesCmd.AddAutoCommand<ReadCmd>();
		quotesCmd.AddAutoCommand<DeleteCmd>();
		quotesCmd.AddAutoCommand<AddCmd>();

		return rootCmd;
	}

	[Command("quotes", "Work with a file that contains quotes.")]
	public class QuotesCmd { }

	[Command("read", "Read and display the file.")]
	public class ReadCmd : FileBaseCmd
	{
		[Option("--delay", "-d", "Delay between lines, specified as milliseconds per character in a line.", DefVal = 42)] // Arity = ArgumentArity.Zero)]
		public int Delay { get; set; }

		[Option("--fgcolor", description: "Foreground color of text displayed on the console.", DefVal = ConsoleColor.White)]
		public ConsoleColor FGColor { get; set; }

		[Option("--light-mode", description: "Background color of text displayed on the console: default is black, light mode is white.")]
		public bool LightMode { get; set; }

		public void Handle()
		{
			if(!TrySetFile())
				return;

			Console.BackgroundColor = LightMode ? ConsoleColor.White : ConsoleColor.Black;
			Console.ForegroundColor = FGColor;

			foreach(string line in File.ReadLines(file.FullName)) {
				WriteLine(line);
				Thread.Sleep(TimeSpan.FromMilliseconds(Delay * line.Length));
			}
		}
	}

	[Command("delete", "Delete lines from the file.")]
	public class DeleteCmd : FileBaseCmd
	{
		[Option("--search-terms", description: "Strings to search for when deleting entries.", Required = true, AllowMultipleArgumentsPerToken = true)]
		public string[] SearchTerms { get; set; }

		public void Handle()
		{
			if(!TrySetFile())
				return;

			WriteLine("Deleting from file");
			var lines = File.ReadLines(file.FullName).Where(line => SearchTerms.All(s => !line.Contains(s)));
			File.WriteAllLines(file.FullName, lines);
		}
	}

	[Command("add", "Add an entry to the file.", Alias = "insert")]
	public class AddCmd : FileBaseCmd
	{
		[Argument("quote", "Text of quote.")]
		public string Quote { get; set; }

		[Argument("byline", "Byline of quote.")]
		public string Byline { get; set; }

		public void Handle()
		{
			if(!TrySetFile())
				return;

			WriteLine("Adding to file");

			using StreamWriter writer = file.AppendText();
			writer.WriteLine($"{Environment.NewLine}{Environment.NewLine}{Quote}");
			writer.WriteLine($"{Environment.NewLine}-{Byline}");
		}
	}

	public class FileBaseCmd
	{
		[Option("--file", "-f", "An option whose argument is parsed as a FileInfo", Required = true)] // Extras = "EasyBuilder.Samples.GetStarted.FHelper"
		public string Filepath { get; set; }

		protected FileInfo file { get; set; }

		public bool TrySetFile()
		{
			Filepath = Filepath?.Trim();

			if(string.IsNullOrEmpty(Filepath)) {
				file = new FileInfo("sampleQuotes.txt");
				return true;
			}
			if(File.Exists(Filepath)) {
				file = new FileInfo(Filepath);
				return true;
			}
			WriteLine("File does not exist");
			return false;
		}

		public ParseResult ParseResult { get; set; }
	}
}

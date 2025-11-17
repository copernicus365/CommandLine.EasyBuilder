using System.CommandLine;

using CommandLine.EasyBuilder;

using static System.Console;

using FileIO = System.IO.File;

namespace EasyBuilder.Samples;

public class GetStartedTutorialApp
{
	public static RootCommand Build()
	{
		RootCommand rootCmd = new("Sample app for System.CommandLine");

		Command quotesCmd = rootCmd.AddAutoCommand<QuotesCmd>();

		Command readCmd = quotesCmd.AddAutoCommand<ReadCmd>();
		quotesCmd.AddAutoCommand<DeleteCmd>();
		quotesCmd.AddAutoCommand<AddCmd>();

		ShowExtraOptions(readCmd);

		return rootCmd;
	}

	/// <summary>Demonstrates how to modify auto constructed options / arguments</summary>
	static void ShowExtraOptions(Command readCmd)
	{
		readCmd.Options.First(o => o.Name == "--fgcolor").Alias("-fg");

		// example if you need the fully generic typed version eg Option<T> ...
		Option<bool> opt = readCmd.Options.First(o => o.Name == "--light-mode") as Option<bool>;
		opt.Alias("-lm");
		//opt.Arity = new ArgumentArity(0, 2);
	}
}

[Command("quotes", "Work with a file that contains quotes.")]
public class QuotesCmd { }

[Command("read", "Read and display the file.")]
public class ReadCmd : FileBase
{
	[Option("--delay", "-d", "Delay between lines, specified as milliseconds per character in a line.", DefVal = 42)] // Arity = ArgumentArity.Zero)]
	public int Delay { get; set; }

	[Option("--fgcolor", description: "Foreground color of text displayed on the console.", DefVal = ConsoleColor.White, MaxArity = 3)]
	public ConsoleColor FGColor { get; set; }

	[Option("--light-mode", description: "Background color of text displayed on the console: default is black, light mode is white.")]
	public bool LightMode { get; set; }

	public void Handle()
	{
		BackgroundColor = LightMode ? ConsoleColor.White : ConsoleColor.Black;
		ForegroundColor = FGColor;

		foreach(string line in FileIO.ReadLines(File.FullName)) {
			WriteLine(line);
			Thread.Sleep(TimeSpan.FromMilliseconds(Delay * line.Length));
		}
		ResetColor(); // Improvement: Reset console colors to avoid affecting future output
	}
}

[Command("delete", "Delete lines from the file.")]
public class DeleteCmd : FileBase
{
	[Option("--search-terms", description: "Strings to search for when deleting entries.", Required = true, AllowMultipleArgumentsPerToken = true)]
	public string[] SearchTerms { get; set; }

	public async Task HandleAsync() // auto looks for a `Handle` or else a `HandleAsync` method
	{
		if(!FileExists())
			return;

		WriteLine("Deleting from file");
		var lines = FileIO.ReadLines(File.FullName).Where(line => SearchTerms.All(s => !line.Contains(s))).ToArray();
		FileIO.WriteAllLines(File.FullName, lines);
	}
}

[Command("add", "Add an entry to the file.", Alias = "insert")]
public class AddCmd : FileBase
{
	[Argument("quote", "Text of quote.")]
	public string Quote { get; set; }

	[Argument("byline", "Byline of quote.")]
	public string Byline { get; set; }

	public void Handle()
	{
		if(!FileExists())
			return;

		WriteLine("Adding to file");

		using StreamWriter writer = File.AppendText();
		writer.WriteLine($"{Environment.NewLine}{Environment.NewLine}{Quote}");
		writer.WriteLine($"{Environment.NewLine}-{Byline}");
	}
}

public class FileBase
{
	/// <summary>Enter "" for `name` to use static method for getting Option or Argument (see below)</summary>
	[Option("")]
	public FileInfo File { get; set; }

	/// <summary>
	/// For advanced options needing a direct Option or Argument (needed for some features like DefaultValueFactory),
	/// make a static function named `Get{PropName}`. May prefix with 'Option' or 'Opt" / "Argument" or "Arg".
	/// </summary>
	public static Option<FileInfo> GetFileOption() =>
		new("--file", "-f") {
			Description = "An option whose argument is parsed as a FileInfo",
			Required = true,
			DefaultValueFactory = result => {
				if(result.Tokens.Count == 0)
					return new FileInfo("sampleQuotes.txt");

				string filePath = result.Tokens.Single().Value;
				if(FileIO.Exists(filePath))
					return new FileInfo(filePath);

				result.AddError("File does not exist");
				return null;
			}
		};

	public bool FileExists() => File != null && File.Exists;

	/// <summary>Get access to `ParseResult`: make a property named `ParseResult` or `ParsedResult`</summary>
	public ParseResult ParseResult { get; set; }
}

using System.CommandLine;

namespace EasyBuilder.Samples;

public class GetStartedTutorial
{
	public RootCommand Build()
	{
		Option<FileInfo> fileOption = new("--file") {
			Description = "An option whose argument is parsed as a FileInfo",
			Required = true,
			DefaultValueFactory = result => {
				if(result.Tokens.Count == 0) {
					return new FileInfo("sampleQuotes.txt");

				}
				string filePath = result.Tokens.Single().Value;
				if(!File.Exists(filePath)) {
					result.AddError("File does not exist");
					return null;
				}
				else {
					return new FileInfo(filePath);
				}
			}
		};

		Option<int> delayOption = new("--delay") {
			Description = "Delay between lines, specified as milliseconds per character in a line.",
			DefaultValueFactory = parseResult => 42
		};
		Option<ConsoleColor> fgcolorOption = new("--fgcolor") {
			Description = "Foreground color of text displayed on the console.",
			DefaultValueFactory = parseResult => ConsoleColor.White
		};
		Option<bool> lightModeOption = new("--light-mode") {
			Description = "Background color of text displayed on the console: default is black, light mode is white."
		};

		Option<string[]> searchTermsOption = new("--search-terms") {
			Description = "Strings to search for when deleting entries.",
			Required = true,
			AllowMultipleArgumentsPerToken = true
		};
		Argument<string> quoteArgument = new("quote") {
			Description = "Text of quote."
		};
		Argument<string> bylineArgument = new("byline") {
			Description = "Byline of quote.",
		};

		RootCommand rootCommand = new("Sample app for System.CommandLine");
		fileOption.Recursive = true;
		rootCommand.Options.Add(fileOption);

		Command quotesCommand = new("quotes", "Work with a file that contains quotes.");
		rootCommand.Subcommands.Add(quotesCommand);

		Command readCommand = new("read", "Read and display the file.")
		{
				delayOption,
				fgcolorOption,
				lightModeOption
		  };
		quotesCommand.Subcommands.Add(readCommand);

		Command deleteCommand = new("delete", "Delete lines from the file.");
		deleteCommand.Options.Add(searchTermsOption);
		quotesCommand.Subcommands.Add(deleteCommand);

		Command addCommand = new("add", "Add an entry to the file.");
		addCommand.Arguments.Add(quoteArgument);
		addCommand.Arguments.Add(bylineArgument);
		addCommand.Aliases.Add("insert");
		quotesCommand.Subcommands.Add(addCommand);

		readCommand.SetAction(parseResult => ReadFile(
			 parseResult.GetValue(fileOption),
			 parseResult.GetValue(delayOption),
			 parseResult.GetValue(fgcolorOption),
			 parseResult.GetValue(lightModeOption)));

		deleteCommand.SetAction(parseResult => DeleteFromFile(
			 parseResult.GetValue(fileOption),
			 parseResult.GetValue(searchTermsOption)));

		addCommand.SetAction(parseResult => AddToFile(
			 parseResult.GetValue(fileOption),
			 parseResult.GetValue(quoteArgument),
			 parseResult.GetValue(bylineArgument))
			 );

		return rootCommand;
		//return rootCommand.Parse(args).Invoke();
	}

	internal void ReadFile(FileInfo file, int delay, ConsoleColor fgColor, bool lightMode)
	{
		Console.BackgroundColor = lightMode ? ConsoleColor.White : ConsoleColor.Black;
		Console.ForegroundColor = fgColor;
		foreach(string line in File.ReadLines(file.FullName)) {
			Console.WriteLine(line);
			Thread.Sleep(TimeSpan.FromMilliseconds(delay * line.Length));
		}
	}

	internal void DeleteFromFile(FileInfo file, string[] searchTerms)
	{
		Console.WriteLine("Deleting from file");

		var lines = File.ReadLines(file.FullName).Where(line => searchTerms.All(s => !line.Contains(s)));
		File.WriteAllLines(file.FullName, lines);
	}

	internal void AddToFile(FileInfo file, string quote, string byline)
	{
		Console.WriteLine("Adding to file");

		using StreamWriter writer = file.AppendText();
		writer.WriteLine($"{Environment.NewLine}{Environment.NewLine}{quote}");
		writer.WriteLine($"{Environment.NewLine}-{byline}");
	}
}

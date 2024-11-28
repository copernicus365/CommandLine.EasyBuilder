using System.CommandLine;
using System.CommandLine.Parsing;

using CommandLine.EasyBuilder;

using static System.Console;

namespace EasyBuilder.Samples;

public class ExampleApp3(string appDescription)
{
	public RootCommand GetApp(bool getNew) => getNew ? BuildNew() : BuildOld();

	public int ExampleNum = 3;

	RootCommand BuildNew()
	{
		RootCommand rootCmd = new(appDescription);

		Command quotesCmd = new("quotes", "Work with a file that contains quotes.");
		rootCmd.AddCommand(quotesCmd);

		Option<FileInfo?> fileOption = new Option<FileInfo?>(
			name: "--file",
			description: "An option whose argument is parsed as a FileInfo",
			isDefault: true,
			parseArgument: ParseFileArg) {
			IsRequired = false
		}
			.Alias("-f")
			.DefaultValue("cool.txt");

		rootCmd.AddGlobalOption(fileOption);

		Command readCmd = new Command("read", "Read and display the file.").Init(
			fileOption,
			new Option<int>(
				name: "--delay",
				description: "Delay between lines, specified as milliseconds per character in a line.",
				getDefaultValue: () => 42)
				.Alias("-d"),
			new Option<ConsoleColor>(
				name: "--fgcolor",
				description: "Foreground color of text displayed on the console.",
				getDefaultValue: () => ConsoleColor.White),
			new Option<bool>(
				name: "--light-mode",
				description: "Background color of text displayed on the console: default is black, light mode is white."),
				handle: async (file, delay, fgcolor, lightMode) => {
					await ReadFile(file!, delay, fgcolor, lightMode);
				},
				quotesCmd);

		Command deleteCmd = new Command("delete", "Delete lines from the file.").Init(
			fileOption,
			new Option<string[]>(
				name: "--search-terms",
				description: "Strings to search for when deleting entries.") {
				IsRequired = true,
				AllowMultipleArgumentsPerToken = true
			},
			handle: (file, searchTerms) => {
				DeleteFromFile(file!, searchTerms);
			},
				quotesCmd);

		new Command("add", "Add an entry to the file.").Init(
			fileOption,
			new Argument<string>(
				name: "quote",
				description: "Text of quote."),
			new Argument<string>(
				name: "byline",
				description: "Byline of quote."),
			handle: (file, quote, byline) => {
				AddToFile(file!, quote, byline);
			},
				quotesCmd)
			.Alias("insert");

		return rootCmd;
	}

	RootCommand BuildOld()
	{
		Option<FileInfo?> fileOption = new(
			name: "--file",
			description: "An option whose argument is parsed as a FileInfo",
			isDefault: true,
			parseArgument: ParseFileArg) {
			IsRequired = false
		};

		fileOption.AddAlias("-f");
		fileOption.SetDefaultValue("cool.txt");

		Option<int> delayOption = new(
			name: "--delay",
			description: "Delay between lines, specified as milliseconds per character in a line.",
			getDefaultValue: () => 42);
		delayOption.AddAlias("-d");

		Option<ConsoleColor> fgcolorOption = new(
			name: "--fgcolor",
			description: "Foreground color of text displayed on the console.",
			getDefaultValue: () => ConsoleColor.White);

		Option<bool> lightModeOption = new(
			name: "--light-mode",
			description: "Background color of text displayed on the console: default is black, light mode is white.");

		Option<string[]> searchTermsOption = new(
			name: "--search-terms",
			description: "Strings to search for when deleting entries.") {
			IsRequired = true,
			AllowMultipleArgumentsPerToken = true
		};

		Argument<string> quoteArgument = new(
			name: "quote",
			description: "Text of quote.");

		Argument<string> bylineArgument = new(
			name: "byline",
			description: "Byline of quote.");

		RootCommand rootCmd = new(appDescription);
		rootCmd.AddGlobalOption(fileOption);

		Command quotesCommand = new("quotes", "Work with a file that contains quotes.");
		rootCmd.AddCommand(quotesCommand);

		Command readCommand = new("read", "Read and display the file."){
			delayOption,
			fgcolorOption,
			lightModeOption
		};
		quotesCommand.AddCommand(readCommand);

		Command deleteCommand = new("delete", "Delete lines from the file.");
		deleteCommand.AddOption(searchTermsOption);
		quotesCommand.AddCommand(deleteCommand);

		Command addCommand = new("add", "Add an entry to the file.");
		addCommand.AddArgument(quoteArgument);
		addCommand.AddArgument(bylineArgument);
		addCommand.AddAlias("insert");
		quotesCommand.AddCommand(addCommand);

		readCommand.SetHandler(async (file, delay, fgcolor, lightMode) => {
			await ReadFile(file!, delay, fgcolor, lightMode);
		},
			fileOption, delayOption, fgcolorOption, lightModeOption);

		deleteCommand.SetHandler((file, searchTerms) => {
			DeleteFromFile(file!, searchTerms);
		},
			fileOption, searchTermsOption);

		addCommand.SetHandler((file, quote, byline) => {
			AddToFile(file!, quote, byline);
		},
			fileOption, quoteArgument, bylineArgument);
		return rootCmd;
	}

	FileInfo ParseFileArg(ArgumentResult arg)
	{
		string? filePath = arg.Tokens.Count == 0
			? "sampleQuotes.txt"
			: arg.Tokens.Single().Value;

		$"Blah: {filePath}".Print();

		if(!File.Exists(filePath)) {
			arg.ErrorMessage = $"File does not!! exist / '{filePath}'";
			return null;
		}
		return new FileInfo(filePath);
	}

	internal async Task ReadFile(
		FileInfo file, int delay, ConsoleColor fgColor, bool lightMode)
	{
		BackgroundColor = lightMode ? ConsoleColor.White : ConsoleColor.Black;
		ForegroundColor = fgColor;
		file.FullName.Print();
		var lines = File.ReadLines(file.FullName).ToList();
		foreach(string line in lines) {
			WriteLine(line);
			await Task.Delay(delay * line.Length);
		};
	}

	internal void DeleteFromFile(FileInfo file, string[] searchTerms)
	{
		string path = file.FullName;

		string[] lines = File.ReadLines(path).ToArray();

		string[] newLines = lines
			.Where(ln => !searchTerms.Any(s => ln.Contains(s)))
			.ToArray();

		// remember! if looking at the sample file, don't look at the source one,
		// but at the one copied to output bin!
		WriteLine($"Deleting from file, orig lines: {lines.Length}, new: {newLines.Length}");

		File.WriteAllLines(path, newLines);
	}

	internal void AddToFile(FileInfo file, string quote, string byline)
	{
		WriteLine("Adding to file");
		using StreamWriter? writer = file.AppendText();
		writer.WriteLine($"{Environment.NewLine}{Environment.NewLine}{quote}");
		writer.WriteLine($"{Environment.NewLine}-{byline}");
		writer.Flush();
	}
}

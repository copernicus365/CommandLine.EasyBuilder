using System.CommandLine;

using CommandLine.EasyBuilder;

using static System.Console;

namespace EasyBuilder.Samples;

public class ExampleApp2(string appDescription)
{
	public RootCommand GetApp(bool getNew)
		=> getNew ? BuildNew() : BuildOld();

	RootCommand BuildNew()
	{
		RootCommand rootCmd = new(appDescription);

		var fileOption = new Option<FileInfo>(
			name: "--file",
			description: "The file to read and display on the console.")
			.Alias("-f");

		// don't need to set this variable / line if extra work not needed!
		Command readCmd = new Command("read", "Read and display the file.").Init(
			fileOption,
			new Option<double>(
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
			handle: async (file, fgcolor, delay, lightMode) => {
				await ReadFile(file!, fgcolor, delay, lightMode);
			},
			rootCmd);
		return rootCmd;
	}

	RootCommand BuildOld()
	{
		Option<FileInfo> fileOption = new(
			name: "--file",
			description: "The file to read and display on the console.");
		fileOption.AddAlias("-f");

		Option<double> delayOption = new(
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

		Command readCmd = new("read", "Read and display the file.") {
			fileOption,
			delayOption,
			fgcolorOption,
			lightModeOption
		};
		readCmd.AddOption(fileOption);

		RootCommand rootCmd = new(appDescription);
		//rootCommand.AddOption(fileOption);
		rootCmd.AddCommand(readCmd);

		readCmd.SetHandler(async (file, fgcolor, delay, lightMode) => {
			await ReadFile(file!, delay, fgcolor, lightMode);
		},
			fileOption, fgcolorOption, delayOption, lightModeOption);
		return rootCmd;
	}

	internal async Task ReadFile(
		FileInfo file, double delay, ConsoleColor fgColor, bool lightMode)
	{
		BackgroundColor = lightMode ? ConsoleColor.White : ConsoleColor.Black;
		ForegroundColor = fgColor;
		List<string> lines = File.ReadLines(file.FullName).ToList();
		foreach(string line in lines) {
			WriteLine(line);
			await Task.Delay(TimeSpan.FromMilliseconds(delay * line.Length));
		};
	}
}

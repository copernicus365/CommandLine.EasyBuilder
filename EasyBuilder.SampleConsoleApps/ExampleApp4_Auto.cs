using System.CommandLine;

using CommandLine.EasyBuilder;
using CommandLine.EasyBuilder.Auto;

namespace EasyBuilder.Samples.Test1;

public class ExampleApp4_Auto(string appDescription)
{
	public RootCommand GetApp()
	{
		RootCommand rootCmd = new(appDescription);
		rootCmd.AddAutoCommand<ReadArgs>();

		Option<FileInfo> nameOpt = new Option<FileInfo>("--file", description: "Display some file..")
			.Alias("-f");


		//rootCmd.AddAutoCommand((ReadArgs w) => w.Handle());
		return rootCmd;
	}
}

[Command(
	"read",
	"Read and display the file")]
public class ReadArgs
{
	[Option(
		"--delay", "-d",
		DefVal = 42.0,
		Description = "Delay between lines, specified as milliseconds per character in a line")]
	public double? Delay { get; set; }

	[Option(
		"--fgcolor",
		description: "Foreground color of text displayed on the console")] //DefVal = ConsoleColor.White,
	public ConsoleColor Foreground { get; set; }

	[Option(
		"--light-mode",
		description: "Background color of text displayed on the console: default is black, light mode is white")]
	public bool? Lightmode { get; set; }

	[Option(
		"--file", "-f",
		Required = true,
		Description = "The file to read and display on the console")]
	public FileInfo File { get; set; }

	public async Task Handle()
	{
		if(Lightmode != null) {
			Console.BackgroundColor = Lightmode.Value ? ConsoleColor.White : ConsoleColor.Black;
		}
		if(Foreground != default)
			Console.ForegroundColor = Foreground;
		//Foreground = ConsoleColor.

		List<string> lines = System.IO.File.ReadLines(File.FullName).ToList();

		foreach(string line in lines) {
			Console.WriteLine(line);
			await Task.Delay(TimeSpan.FromMilliseconds(Delay.Value * line.Length));
		};
	}
}

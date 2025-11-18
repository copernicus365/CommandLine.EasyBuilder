using System.CommandLine;

using CommandLine.EasyBuilder;

namespace EasyBuilder.Samples;

public class ReadFileApp
{
	public static RootCommand Build()
	{
		RootCommand rootCmd = [];
		rootCmd.AddAutoCommand<ReadFileCmd>();

		Option<FileInfo> nameOpt = new Option<FileInfo>("--file")
			.Alias("-f");
		nameOpt.Description = "Display some file..";

		//rootCmd.AddAutoCommand((ReadArgs w) => w.Handle());
		return rootCmd;
	}
}

[Command("read-file", "Read and display the file")]
public class ReadFileCmd
{
	[Option("--delay", "-d", DefVal = 42.0, Description = "Delay between lines, specified as milliseconds per character in a line")]
	public double? Delay { get; set; }

	[Option("--fgcolor", description: "Foreground color of text displayed on the console")] //DefVal = ConsoleColor.White,
	public ConsoleColor Foreground { get; set; }

	[Option("--light-mode", description: "Background color of text displayed on the console: default is black, light mode is white")]
	public bool? Lightmode { get; set; }

	[Option("--file", "-f", Required = true, Description = "The file to read and display on the console")]
	public FileInfo File { get; set; }

	public async Task Handle()
	{
		if(Lightmode != null) {
			Console.BackgroundColor = Lightmode.Value ? ConsoleColor.White : ConsoleColor.Black;
		}
		if(Foreground != default)
			Console.ForegroundColor = Foreground;

		List<string> lines = System.IO.File.ReadLines(File.FullName).ToList();

		foreach(string line in lines) {
			Console.WriteLine(line);
			await Task.Delay(TimeSpan.FromMilliseconds(Delay.Value * line.Length));
		}
	}
}

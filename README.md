# CommandLine.EasyBuilder

Building upon the terrific `System.CommandLine`, `CommandLine.EasyBuilder` makes it easier than ever to make a command line app, offering view-model type auto-binding of input command-line options and arguments onto the properties of a POCO class. Simply decorate a class with a `Command` attribute, and decorate any of its properties with `Option` or `Argument` attributes, and quite literally, `CommandLine.EasyBuilder` takes care of the rest! More often than not this can replace the need to manually wire up via the traditional way. This also makes your command-line much easier to understand, to edit and to alter, and so forth. It's basically declarative decorations on a class and it's properties, building on top of all the tremendous work that `System.CommandLine` already does.

```csharp
using System.CommandLine;
using CommandLine.EasyBuilder;
using static System.Console;

namespace EasyBuilder.Samples;

public class ExampleApp_HelloWorld
{
	public static RootCommand GetApp()
	{
		RootCommand rootCmd = new("Command line is cool");
		rootCmd.AddAutoCommand<HellowWorld>();
		return rootCmd;
	}
}

[Command("hello", "Hello commandline world!")]
public class HellowWorld
{
	[Option("--name", "-n", Required = true)]
	public string Name { get; set; }

	[Option(name: "--age", DefVal = 42)]
	public int Age { get; set; }

	[Option("--animal", "-a", Required = true)]
	public FavoriteAnimal FavAnimal { get; set; } = FavoriteAnimal.Cheetah;

	public void Handle()
		=> WriteLine($"Hello {Name} ({Age}), glad to see you love {FavAnimal}s!");
}

public enum FavoriteAnimal { None = 0, Dog = 1, Cat = 2, Cheetah = 3, Rhino = 4 }
```

In this example, simply add this class to a `Rootcommand` (or sub-command `Command`): `rootCmd.AddAutoCommand<HelloWorld>()`, and all the magic is taken care of. From there, run the traditional way, once you have a `Rootcommand`. We're not demonstrating that here, because none of that changes, but you can see the example app.

Add either a `void Handle` or `Task HandleAsync` method to your POCO class, and they will be auto-found and called when `Invoke` or `InvokeAsync` is called on the `ParseResult` (ie same as usual).

Other examples are shown in the `EasyBuilder.SampleConsoleApps` app.

For a larger example, see `GetStartedTutorialSimple.cs` shown below. This takes the final app demonstrated in this `System.CommandLine` [getting started tutorial](https://learn.microsoft.com/en-us/dotnet/standard/commandline/get-started-tutorial), but instead builds it in the `CommandLine.EasyBuilder` style with POCO command classes. This example with inline comments can serve as documentation for a number of scenarios.

```csharp
using System.CommandLine;
using CommandLine.EasyBuilder;
using static System.Console;
using FileIO = System.IO.File;

namespace EasyBuilder.Samples;

public class GetStartedTutorial_Auto
{
	public RootCommand GetApp()
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
	void ShowExtraOptions(Command readCmd)
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
	[Option<FileInfo>("")]
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
```

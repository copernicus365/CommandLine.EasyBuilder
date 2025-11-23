# CommandLine.EasyBuilder

Building upon the terrific `System.CommandLine`, `CommandLine.EasyBuilder` makes it easier than ever to make a command line app, offering view-model type of auto-binding of command-line options and arguments to the properties of a POCO class. Simply decorate a class with a `Command` attribute, and decorate any of its properties with `Option` or `Argument` attributes, and quite literally, `CommandLine.EasyBuilder` takes care of the rest! More often than not this can replace the need to manually wire up via the traditional way. This also makes your command-line much easier to understand, to edit and to alter, and so forth. It's basically declarative decorations on a class and it's properties, building on top of all the tremendous work that `System.CommandLine` already does.

## Simple Hello World example

```csharp
using System.CommandLine;

using CommandLine.EasyBuilder;

namespace EasyBuilder.Samples;

[Command("hello", "Hello commandline world!")]
public class HelloWorldCmd
{
	[Option("--name", "-n", required: true)]
	public string Name { get; set; }

	[Option("--age")] //, DefVal = 42)]
	public int Age { get; set; } = 12;

	[Option("--animal", "-a", required: true)]
	public FavoriteAnimal FavAnimal { get; set; } = FavoriteAnimal.Cheetah;

	public void Handle()
		=> WriteLine($"Hello {Name} ({Age}), glad to see you love {FavAnimal}s!");
}

public enum FavoriteAnimal { None = 0, Dog = 1, Cat = 2, Cheetah = 3, Rhino = 4 }

public class HelloWorldApp
{
	// rename `Main_Run` to `Main` to run as a standalone app
	public static async Task<int> Main_Run(string[] args)
	{
		RootCommand root = GetApp(false);
		await BasicCLILoop.Run(root, args, doLoop: true);
		return 0;
	}

	public static RootCommand GetApp(bool makeSubcommand)
	{
		RootCommand rootCmd = new("Command line is cool");
		if(makeSubcommand)
			rootCmd.AddAutoCommand<HelloWorldCmd>();
		else
			rootCmd.SetAutoCommand<HelloWorldCmd>();
		return rootCmd;
	}
}
```

In this example, do either:

* `rootCmd.AddAutoCommand<HelloWorldCmd>();` - to add as a subcommand, or
* `rootCmd.SetAutoCommand<HelloWorldCmd>();` - to add options directly upon the root command (or any other `Command`)

And all the magic is taken care of. From there, run the traditional way, once you have a `Rootcommand`.

Our `HelloWorldApp.Run()` function demonstrates this full simple app above, but NOTE WELL that after `GetApp()`, there's nothing unique to this library.

Add either a `void Handle` or `Task HandleAsync` method to your POCO class, and they will be auto-found and called when `Invoke` or `InvokeAsync` is called on the `ParseResult` (ie same as usual).

## Example output 1: Set on root command (not a subcommand)


```txt
Description:
  Hello commandline world!

Usage:
  EasyBuilder.Samples [options]

Options:
  -n, --name <name> (REQUIRED)
  --age <age>
  -a, --animal <Cat|Cheetah|Dog|None|Rhino> (REQUIRED)
  -?, -h, --help                                        Show help and usage information
  --version                                             Show version information

>> -n
Error: Required argument missing for option: '-n'.
Error: Option '--animal' is required.

>> -n Joe -a Cheeeetah
Error: Cannot parse argument 'Cheeeetah' for option '--animal' as expected type 'EasyBuilder.Samples.FavoriteAnimal'. Did you mean one of the following?
Cat
Cheetah
Dog
None
Rhino

>> -n Joe -a Rhino
Hello Joe (0), glad to see you love Rhinos!

>> -n Joe -a Rhino --age 27
Hello Joe (27), glad to see you love Rhinos!

```

## Example output 2: Set as subcommand

```txt
Description:
  Command line is cool

Usage:
  EasyBuilder.Samples [command] [options]

Options:
  -?, -h, --help  Show help and usage information
  --version       Show version information

Commands:
  hello  Hello commandline world!


>> hello
Error: Option '--name' is required.
Error: Option '--animal' is required.

>> hello -h
Description:
  Hello commandline world!

Usage:
  EasyBuilder.Samples hello [options]

Options:
  -n, --name <name> (REQUIRED)
  --age <age>
  -a, --animal <Cat|Cheetah|Dog|None|Rhino> (REQUIRED)
  -?, -h, --help                                        Show help and usage information


>> hello -n Joey --age 59 -a Cat
Hello Joey (59), glad to see you love Cats!

```

## Model dependency injection 

DI is now possible! You can set the type to model instance getter in a single line at startup (agnostic to any given DI system), ex:

`BuilderDI.ModelInstanceGetter = type => ActivatorUtilities.CreateInstance(host.Services, type);`

## Top-level app example + DI

Here's the top-level app example in samples project:

```csharp
using CommandLine.EasyBuilder;

using EasyBuilder.Samples;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// FOR TOP-LEVEL (non-Main) way w/Hosting. To use: UNCOMMENT CODE BODY below and rename `Main`

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<ICoolSvc, CoolSvc>();

var host = builder.Build();

BuilderDI.ModelInstanceGetter = type => ActivatorUtilities.CreateInstance(host.Services, type);
// or if BuilderDIX.cs added to proj: //host.Services.SetEasyBuilderDI();

var rootCmd = HelloDIApp.GetApp();

await BasicCLILoop.Run(rootCmd, args, doLoop: true);
```

For `CoolSvc` example above, see [HelloDICmd.cs](EasyBuilder.SampleConsoleApps/Samples/HelloDICmd.cs)

## Full GetStartedTutorialApp example

Now picks up auto-property (/'default value') initializers! So either of the following ways sets a default value now:

```csharp
[Option("--age", DefVal = 11)]
public int Age1 { get; set; }
```

or

```csharp
[Option("--age")]
public int Age2 { get; set; } = 22;
```

## Full GetStartedTutorialApp example

Other examples are shown in the `EasyBuilder.SampleConsoleApps` app.

But for a larger example, see `GetStartedTutorialSimple.cs` shown below. This takes the final app demonstrated in this `System.CommandLine` [getting started tutorial](https://learn.microsoft.com/en-us/dotnet/standard/commandline/get-started-tutorial), but instead builds it in the `CommandLine.EasyBuilder` style with POCO command classes. This example with inline comments can serve as documentation for a number of scenarios.


```csharp
using System.CommandLine;

using CommandLine.EasyBuilder;

using FileIO = System.IO.File;

namespace EasyBuilder.Samples.Tutorial;

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

	/// <summary>
	/// For cases where you need to do extra setup beyond what the attributes provide
	/// (as of course plenty of scenarios cannot be handled by an Attribute alone):
	/// NOTE that `AddAutoCommand` returns the created <see cref="Command"/>, and from
	/// there you can edit or alter it and its <see cref="Option"/>s or <see cref="Argument"/>s, eg:
	/// `cmd.Options.First(o => o.Name == "--name") ... `.
	/// <para />
	/// Note that it is also possible via the EasyBuilder model class to
	/// directly set a models properties to a full <see cref="Option{T}"/> or <see cref="Argument{T}"/>:
	/// SEE example below: <see cref="FileBase.GetFileOption"/>. In short, make a static function named
	/// `Get{PropName}`: eg `static Option<FileInfo> GetFile() => ...` (or named `GetFileOption` or `GetFileOpt`).
	/// This allows one to fully control the instantiation of an Option{T} (or Argument{T}) at the start.
	/// </summary>
	static void ShowExtraOptions(Command cmd)
	{
		cmd.Options.First(o => o.Name == "--fgcolor").Alias("-fg");

		// or get the option var directly:
		Option opt = cmd.Options.First(o => o.Name == "--light-mode");
		opt.Alias("-lm");
		//opt.Arity = new ArgumentArity(0, 2);
		//opt.AcceptOnlyFromAmong(["a", "b"]); // not even sure proper example values, but you get the idea!

		// Or get typed version, ex: CustomParser only editable on typed Option{T}:
		Option<bool> optLM = opt as Option<bool>;

		optLM.CustomParser = result => {
			if(result.Tokens.Count == 0)
				return true; // default to true if no arg given
			string val = result.Tokens.Single().Value.ToLowerInvariant();
			return val is "1" or "true" or "yes" or "on";
		};
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
	/// <summary>Enter "" for `name` (or `name: null`) to use static method for getting Option or Argument (see below)</summary>
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
```

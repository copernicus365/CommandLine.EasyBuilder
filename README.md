# CommandLine.EasyBuilder

Extending upon the terrific `System.CommandLine` library, `CommandLine.EasyBuilder` offers a more functional and user friendly way of building up `System.CommandLine`'s types. While `System.CommandLine` is a very solid library that is a joy to use once it is running, the one glaring problem is when one is on the end of specifying or building it up. On that front there is much to be desired.

The bad thing about that is, I envision command-line apps could start taking pride of place in the dotnet world, where many console apps could simply and easily be converted to being command-line apps (note: command-line apps do *not* have to be a single run thing, nor do they have to take their arguments from Main's args. And nor do they have to auto close after running once. In fact they can just as easily be run in a loop with new input per round just as easily, actually, simply identically, as we do with console apps.)

But currently, in my view people aren't going to do this nearly as often as they would because of the mundane and tedious manner in which `System.CommandLine` requires in order to specifiy and build up the command line parameters.

## Examples

Before: to specify a single `Option<T>`, note how one can't just set an option as if it were a single property within a bigger class (in this case: `Command`). It would be as if when building a C# class, to add a property (which corresponds to an option), before the class is even specified, you have to make a standalone property 30 lines beforehand, and then 30 lines later, ADD that property to that class. And THEN 30 or so lines later, again add the property variable into a handler when one needs to call a function... DISJOINTED INSANITY! (I hope no one's offended here: I fully understand why these things weren't added yet: Because 1. it was hard to do, and 2. often such things impose limitations, or require compromises, things which the mainstream library may not want want to hastle with).

So let's consider one example, focusing on a single option, highlight the variable **`delayOption`**:

OLD:

```csharp
Option<double> delayOption = new(
	name: "--delay",
	description: "Delay between lines, specified as milliseconds per character in a line.",
	getDefaultValue: () => 42);

delayOption.AddAlias("-d");

//...

Command readCmd = new("read", "Read and display the file.") {
	fileOption,
	delayOption,
	fgcolorOption,
	lightModeOption
};

readCmd.SetHandler(async (file, fgcolor, delay, lightMode) => {
	await ReadFile(file!, delay, fgcolor, lightMode);
},
	fileOption, fgcolorOption, delayOption, lightModeOption);

```

NEW:

```csharp
Command readCmd = new Command("read", "Read and display the file.").Init(
	new Option<double>(
		name: "--delay",
		description: "Delay between lines, specified as milliseconds per character in a line.",
		getDefaultValue: () => 42)
		.Alias("-d"),
	handle: async (file, fgcolor, delay, lightMode) => {
		await ReadFile(file!, fgcolor, delay, lightMode);
	},
	rootCmd);
```

No duplications, all DRY! All far more functional, while in this way loosing virtually nothing. You can just as well for example build that option as a separate variable still.

There are multiple full examples like this in the sample library. It is true that this way takes far fewer lines of code (almost half), but more key is the cognitive simplification and decrease in repetition.

## Auto Examples

Even better, we now have what I'm calling `Auto` types, by means of class and property attributes, in order to make this even simpler and cleaner, all while, very importantly, simply compiling down to the same types as above (i.e. `Option<T>`, `Argument<T>`, `Command<T>`, etc.

```
[Command(
	"read",
	"Read and display the file")]
public class ReadArgs
{
	[Option(
		"--delay", "-d",
		description: "Delay between lines, specified as milliseconds per character in a line",
		DefVal = 42.0)]
	public double? Delay { get; set; }

	// ...

	public async Task Handle()
		=> await ReadFile(file!, fgcolor, delay, lightMode);
}
```

To then use the above class to build it into a command line, it is literally as simple as this:

```
RootCommand rcmd = new("Who said command line isn't cool?!");

rcmd.AddAutoCommand<ReadArgs>();
```

I would like to strongly emphasize this again: Beyond building the app with these new functional and POCO type classes, *everything* then builds down to the same types: `Option<T>`, `Argument<T>`, `Command<T>` etc. One can then do the same with those types as they ever would have done with `System.CommandLine`.

As an aside: *personally* I would argue that the classes one uses for specifying the app, like `ReadArgs` above, are better to be seen as a DTO / data transfer object types like we see in the web and ASP.NET world. In other words, probably better to let the `ReadArgs` class be limited to the singular problem of taking initial input from the command line for any more serious use case.

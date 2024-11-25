using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Completions;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.CommandLine.NamingConventionBinder;
using System.Reflection;

namespace EasyBuilder.Samples.V1;

// https://learn.microsoft.com/en-us/dotnet/standard/commandline/model-binding#parameter-binding-more-than-8-options-and-arguments

public class Program
{
	public RootCommand GetApp()
	{
		var fileOption = new Option<FileInfo?>(
			  name: "--file",
			  description: "An option whose argument is parsed as a FileInfo",
			  getDefaultValue: () => new FileInfo("scl.runtimeconfig.json"));

		var firstNameOption = new Option<string>(
			  name: "--first-name",
			  description: "Person.FirstName");

		var lastNameOption = new Option<string>(
			  name: "--last-name",
			  description: "Person.LastName");

		var rootCommand = new RootCommand();
		rootCommand.Add(fileOption);
		rootCommand.Add(firstNameOption);
		rootCommand.Add(lastNameOption);

		rootCommand.SetHandler((fileOptionValue, person) =>
		{
			DoRootCommand(fileOptionValue, person);
		},
			fileOption, new PersonBinder(firstNameOption, lastNameOption));

		return rootCommand;
	}

	public static void DoRootCommand(FileInfo? aFile, Person aPerson)
	{
		Console.WriteLine($"File = {aFile?.FullName}");
		Console.WriteLine($"Person = {aPerson?.FirstName} {aPerson?.LastName}");
	}

	public class Person
	{
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
	}

	public class PersonBinder : BinderBase<Person>
	{
		private readonly Option<string> _firstNameOption;
		private readonly Option<string> _lastNameOption;

		public PersonBinder(Option<string> firstNameOption, Option<string> lastNameOption)
		{
			_firstNameOption = firstNameOption;
			_lastNameOption = lastNameOption;

			//PropertyInfo property = null;
			//Person p1 = new();
			//property.SetValue(p1, "");
		}

		protected override Person GetBoundValue(BindingContext bindingContext) =>
			new Person {
				FirstName = bindingContext.ParseResult.GetValueForOption(_firstNameOption),
				LastName = bindingContext.ParseResult.GetValueForOption(_lastNameOption)
			};
	}

	public class ArrTypeBinder : BinderBase<Dictionary<string, object>>
	{
		protected override Dictionary<string, object> GetBoundValue(BindingContext bindingContext)
		{
			throw new NotImplementedException();
		}
	}
}
//public class T1
//{
//	public void dd()
//	{
//		//System.CommandLine.NamingConventionBinder.ConstructorDescriptor
//		//ICommandOptionsHandler
//	}

//}

//public class HelloCommand : Command<HelloCommandOptions, HelloCommandOptionsHandler>
//{
//	// Keep the hard dependency on System.CommandLine here
//	public HelloCommand()
//		: base("hello", "Say hello to someone")
//	{
//		this.AddOption(new Option<string>("--to", "The person to say hello to"));
//	}
//}

//public class HelloCommandOptions : ICommandOptions
//{
//	// Automatic binding with System.CommandLine.NamingConventionBinder
//	public string To { get; set; } = string.Empty;
//}

//public class HelloCommandOptionsHandler : ICommandOptionsHandler<HelloCommandOptions>
//{
//	private readonly IConsole _console;

//	// Inject anything here, no more hard dependency on System.CommandLine
//	public HelloCommandOptionsHandler(IConsole console)
//	{
//		this._console = console;
//	}

//	public Task<int> HandleAsync(HelloCommandOptions options, CancellationToken cancellationToken)
//	{
//		this._console.WriteLine($"Hello {options.To}!");
//		return Task.FromResult(0);
//	}
//}

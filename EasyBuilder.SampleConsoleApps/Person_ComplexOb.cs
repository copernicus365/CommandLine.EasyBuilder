//using System.CommandLine;
//using System.CommandLine.Binding;

//namespace EasyBuilder.Samples.V1;

//// https://learn.microsoft.com/en-us/dotnet/standard/commandline/model-binding#parameter-binding-more-than-8-options-and-arguments

//public class Program
//{
//	public RootCommand GetApp()
//	{
//		var fileOption = new Option<FileInfo?>(
//			  name: "--file",
//			  description: "An option whose argument is parsed as a FileInfo",
//			  getDefaultValue: () => new FileInfo("scl.runtimeconfig.json"));

//		var firstNameOption = new Option<string>(
//			  name: "--first-name",
//			  description: "Person.FirstName");

//		var lastNameOption = new Option<string>(
//			  name: "--last-name",
//			  description: "Person.LastName");

//		var rootCommand = new RootCommand();
//		rootCommand.Add(fileOption);
//		rootCommand.Add(firstNameOption);
//		rootCommand.Add(lastNameOption);

//		rootCommand.SetHandler((fileOptionValue, person) => {
//			DoRootCommand(fileOptionValue, person);
//		},
//			fileOption, new PersonBinder(firstNameOption, lastNameOption));

//		return rootCommand;
//	}

//	public static void DoRootCommand(FileInfo? aFile, Person aPerson)
//	{
//		Console.WriteLine($"File = {aFile?.FullName}");
//		Console.WriteLine($"Person = {aPerson?.FirstName} {aPerson?.LastName}");
//	}

//	public class Person
//	{
//		public string? FirstName { get; set; }
//		public string? LastName { get; set; }
//	}

//	public class PersonBinder : BinderBase<Person>
//	{
//		private readonly Option<string> _firstNameOption;
//		private readonly Option<string> _lastNameOption;

//		public PersonBinder(Option<string> firstNameOption, Option<string> lastNameOption)
//		{
//			_firstNameOption = firstNameOption;
//			_lastNameOption = lastNameOption;

//			//PropertyInfo property = null;
//			//Person p1 = new();
//			//property.SetValue(p1, "");
//		}

//		protected override Person GetBoundValue(BindingContext bindingContext) =>
//			new Person {
//				FirstName = bindingContext.ParseResult.GetValueForOption(_firstNameOption),
//				LastName = bindingContext.ParseResult.GetValueForOption(_lastNameOption)
//			};
//	}

//	public class ArrTypeBinder : BinderBase<Dictionary<string, object>>
//	{
//		protected override Dictionary<string, object> GetBoundValue(BindingContext bindingContext)
//			=> throw new NotImplementedException();
//	}
//}

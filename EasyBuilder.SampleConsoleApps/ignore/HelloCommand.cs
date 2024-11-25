//namespace EasyBuilder.Samples;

//public class HelloCommand : Command<HelloCommandOptions, HelloCommandOptionsHandler>
//{
//	// Keep the hard dependency on System.CommandLine here
//	public HelloCommand()
//		: base("hello", "Say hello to someone")
//	{
//		//this.AddOption(new Option<string>("--to", "The person to say hello to"));
//	}
//}

//public class HelloCommandOptions : ICommandOptions
//{
//	void DD()
//	{
		
//	}
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


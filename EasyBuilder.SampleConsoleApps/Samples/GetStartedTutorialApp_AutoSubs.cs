using System.CommandLine;

using CommandLine.EasyBuilder;

namespace EasyBuilder.Samples.Tutorial;

public class GetStartedTutorialApp_AutoSubs
{
	public static RootCommand Build()
	{
		RootCommand rootCmd = new("Sample app for System.CommandLine");
		rootCmd.AddAutoCommand<QuotesAutoSubsCmd>();
		return rootCmd;
	}
}

/// <summary>
/// UPCOMING?!?! auto-add sub-commands?! Contemplating...
/// Probable rules:
/// 1. no other auto-options found, and no Handle/HandleAsync method found
/// 2. only then we see if any properties are sub-commands
/// 3. when running, this instance class *never* has these properties actually set,
/// they are purely for discovery / for building at startup. Their values remain null.
/// </summary>
[Command("quotes-auto-subcommands", "Work with a file that contains quotes.", Alias = "quotes-auto")]
public class QuotesAutoSubsCmd
{
	public ReadCmd Read { get; set; }
	public DeleteCmd Delete { get; set; }
	public AddCmd Add { get; set; }
}

using System.CommandLine;

using CommandLine.EasyBuilder;

namespace EasyBuilder.Samples.Test1;

public class ExampleApp4(string appDescription)
{
	public RootCommand GetApp()
	{
		RootCommand rootCmd = new(appDescription);

		Command duderCmd = new Duder().InitCommand(p => {
			$"Person! {p.FirstName} {p.LastName} is {p.Age} years old".Print();
		}, rootCmd);

		return rootCmd;
	}
}

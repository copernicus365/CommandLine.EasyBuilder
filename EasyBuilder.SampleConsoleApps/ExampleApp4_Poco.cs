using System.CommandLine;

using CommandLine.EasyBuilder;
using CommandLine.EasyBuilder.Auto;

namespace EasyBuilder.Samples.Test1;

public class ExampleApp4_Poco(string appDescription)
{
	public RootCommand GetApp()
	{
		//!!! For real!!!

		RootCommand rootCmd = new(appDescription);

		rootCmd.AddCommand<PersonArgs>(p => {
			$"Person! {p.FirstName} {p.LastName} is {p.Age} years old".Print();
		});

		rootCmd.AddCommand<WaterTank>(w => {
			$"Water input: {w.Name} {w.Level} -- {w.FunnyFile.FullName}".Print();

			string fContent = File.ReadAllText(w.FunnyFile.FullName);

			fContent.Print();
		});
		return rootCmd;
	}
}

[Control(Name = "water", Alias = "wawa", Description = "Funny watertank fish filler")]
public class WaterTank
{
	[Option(Name = "--name", Description = "Tank name", Required = true)]
	public string Name { get; set; }

	[Option(Name = "--level", Alias = "-l", Description = "Tank level in deciwaters", Required = true, DefVal = 24)]
	public int Level { get; set; }

	[Option(Name = "--funny-file", Alias = "-f", Description = "Silly path", Required = true)]
	public FileInfo FunnyFile { get; set; }
}

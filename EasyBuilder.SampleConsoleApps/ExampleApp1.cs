//using System.CommandLine;

//using CommandLine.EasyBuilder;

//using static System.Console;

//namespace EasyBuilder.Samples;

//public class ExampleApp1(string appDescription)
//{
//	public RootCommand GetApp(bool getNew)
//		=> getNew ? BuildNew() : BuildOld();

//	RootCommand BuildNew()
//	{
//		RootCommand rootCmd = new(appDescription);
//		//rootCmd.TreatUnmatchedTokensAsErrors = false;

//		rootCmd.Init(
//			new Option<FileInfo?>(
//				name: "--file",
//				description: "The file to read and display on the console."),
//			handle: (file) => {
//				ReadFile(file!);
//			});

//		return rootCmd;
//	}

//	RootCommand BuildOld()
//	{
//		RootCommand rootCmd = new(appDescription);
//		//rootCommand.TreatUnmatchedTokensAsErrors = false;

//		Option<FileInfo?> fileOption = new(
//			name: "--file",
//			description: "The file to read and display on the console.");

//		rootCmd.AddOption(fileOption);

//		rootCmd.SetHandler((file) => {
//			ReadFile(file!);
//		},
//			fileOption);

//		return rootCmd;
//	}

//	void ReadFile(FileInfo file)
//	{
//		if(file == null)
//			return;

//		string[] lines = File.ReadLines(file.FullName)
//			//.Take(12)
//			.ToArray();

//		foreach(string line in lines)
//			WriteLine(line);
//	}
//}

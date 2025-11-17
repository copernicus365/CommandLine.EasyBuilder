using CommandLine.EasyBuilder;

namespace EasyBuilder.Samples;

[Command("demo-kind", "Change demo app CLI kind")]
public class ChangeDemoAppCLIKindCmd
{
	[Argument("kind", required: true)]
	public SampleAppKind Kind { get; set; }

	public void Handle()
	{
		SampleAppKind curr = Program.Kind;
		Program.Kind = Kind;
		Program.ResetKind = true;
		Console.WriteLine($"Sample CLI app changed: {curr} to {Kind}");
	}
}

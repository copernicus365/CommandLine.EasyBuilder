using CommandLine.EasyBuilder;

namespace EasyBuilder.Samples;

[Command("demo", "Change demo app CLI kind")]
public class DemoAppCmd
{
	[Argument("kind", required: true)]
	public SampleAppKind Kind { get; set; }

	[Option("--as-root", "-r")]
	public bool? SetAsRoot { get; set; }

	public void Handle()
	{
		SampleAppKind curr = Program.Kind;
		Program.Kind = Kind;
		Program.ResetKind = true;

		if(SetAsRoot != null)
			Program.SampleAppsSetAsRoot = SetAsRoot.Value;

		WriteLine($"Sample CLI app changed: {curr} to {Kind} (root: {(Program.SampleAppsSetAsRoot ? "t" : "f")})");
	}
}

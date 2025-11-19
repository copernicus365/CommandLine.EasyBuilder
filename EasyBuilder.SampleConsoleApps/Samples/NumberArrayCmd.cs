using CommandLine.EasyBuilder;

namespace EasyBuilder.Samples;

[Command("numbers-array-test", "Test numbers arrays input", Alias = "nums")]
public class NumberArrayCmd
{
	[Option("--name", "-n", required: true)]
	public string Name { get; set; }

	[Option("--delays", "-d", required: true)]
	public double[] Delays { get; set; }

	public void Handle()
		=> WriteLine($"Hello {Name}: {Delays?.JoinToString(",")}");
}

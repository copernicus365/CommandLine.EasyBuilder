using CommandLine.EasyBuilder;

namespace EasyBuilder.Samples;

[Command("numbers-array-test", "Test numbers arrays input", Alias = "nums")]
public class NumberArrayCmd
{
	[Option("--name", "-n", Required = true)]
	public string Name { get; set; }

	[Option("--delays", "-d")]
	public double[] Delays { get; set; }

	public void Handle()
		=> Console.WriteLine($"Hello {Name}: {Delays?.JoinToString(",")}");
}

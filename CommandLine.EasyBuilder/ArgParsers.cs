using System.CommandLine.Parsing;

namespace CommandLine.EasyBuilder;

public static class ArgParsers
{
	/// <summary>
	/// Use in Option constructor set to `parseArgument`,
	/// for parsing a string value that should be parsed as an
	/// integer array.
	/// </summary>
	/// <param name="arg">Arg</param>
	public static int[] IntArray(ArgumentResult arg)
		=> ArrayParser(arg, str => int.TryParse(str, out int val) ? (int?)val : null);

	public static double[] DoubleArray(ArgumentResult arg)
		=> ArrayParser(arg, str => double.TryParse(str, out double val) ? (double?)val : null);

	public static long[] LongArray(ArgumentResult arg)
		=> ArrayParser(arg, str => long.TryParse(str, out long val) ? (long?)val : null);

	public static decimal[] DecimalArray(ArgumentResult arg)
		=> ArrayParser(arg, str => decimal.TryParse(str, out decimal val) ? (decimal?)val : null);

	public static T[] ArrayParser<T>(ArgumentResult arg, Func<string, T?> conv) where T : struct
	{
		string str = arg.Tokens.Single().Value;//?.Trim();
		if(str == null)
			return null;

		// It turns out "quotes" are already removed by input framework, so no use striping them

		if(str.Length > 1 && str[0] == '[')
			str = str[1..^1];

		string[] svals = str
			.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		T[] arr = new T[svals.Length];

		for(int i = 0; i < svals.Length; i++) {
			string s = svals[i];
			T? val = conv(s);
			if(val == null) {
				arg.ErrorMessage = $"Value is out of range or invalid: {s}";
				return null;
			}
			arr[i] = val.Value;
		}

		return arr;
	}
}

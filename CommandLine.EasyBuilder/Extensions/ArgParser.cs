using System.Globalization;
using System.Numerics;

namespace CommandLine.EasyBuilder;

public static class ArgParser
{
	public static T[] ParseNumberArray<T>(string input) where T : INumber<T>
	{
		if(!TryParseNumberArray<T>(input, out T[] values))
			throw new ArgumentException("Invalid number array");
		return values;
	}

	/// <summary>
	/// Trys to parse a string of comma-separated numbers of type T.
	/// Null or empty is valid (returns true, with values = null). Trimming and
	/// empty entries are ignored.
	/// </summary>
	/// <typeparam name="T">Number type</typeparam>
	/// <param name="input">Input string</param>
	/// <param name="values">T[] values</param>
	/// <returns>True if null or whitespace or if all items parsed. False if any invalid parses</returns>
	public static bool TryParseNumberArray<T>(string input, out T[] values)
		where T : INumber<T>
	{
		values = null;
		if(string.IsNullOrWhiteSpace(input))
			return true;

		var provider = CultureInfo.InvariantCulture; // Use invariant for consistent decimal separators

		string[] parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		T[] arr = new T[parts.Length];

		for(int i = 0; i < parts.Length; i++) {
			string part = parts[i];
			if(!T.TryParse(part, provider, out T val))
				return false;
			arr[i] = val;
		}

		values = arr;
		return true;
	}
}

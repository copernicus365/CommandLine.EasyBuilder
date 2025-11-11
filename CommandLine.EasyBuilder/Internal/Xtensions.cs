using System.Text;

namespace CommandLine.EasyBuilder.Private;

internal static class Xtensions
{
	public static string JoinToString<T>(this IEnumerable<T> thisEnumerable, string separator = ",")
	{
		if(thisEnumerable == null) return null;
		if(separator == null) throw new ArgumentNullException("separator");

		StringBuilder sb = new("");

		foreach(T item in thisEnumerable)
			sb.Append(item.ToString() + separator);

		if(sb.Length == 0)
			return "";
		if(sb.Length > separator.Length)
			sb.Length = sb.Length - separator.Length;

		return _jsr(sb.ToString(), separator);
	}

	static string _jsr(string result, string separator = ",")
	{
		return (result.IsNulle() || result != separator)
			? result
			: "";
	}

	internal static bool NotNulle(this string str)
		=> str != null && str.Length != 0;

	internal static bool IsNulle(this string str)
		=> str == null || str.Length == 0;

	internal static bool IsNulle<T>(this T[] arr)
		=> arr == null || arr.Length == 0;

	internal static bool NotNulle<T>(this T[] arr)
		=> arr != null && arr.Length > 0;
}

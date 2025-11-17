using System.CommandLine;
using System.CommandLine.Parsing;
using System.Reflection;

namespace CommandLine.EasyBuilder.Internal;

internal class SetPropValue
{
	/// <summary>
	/// Critical method used per Invoke to parse an individual property
	/// (<see cref="CmdProp"/>), and to set it's value onto the input <paramref name="model"/>
	/// model's corresponding property.
	/// </summary>
	/// <param name="parseRes"></param>
	/// <param name="p"></param>
	/// <param name="model"></param>
	/// <param name="errorMsg">
	/// Error message if error encountered.
	/// Currently, we are only needing to set this in our special case of value handling
	/// IsNumberArray. But other scenarios might later arise. The key is that if parsing error
	/// is encountered, we do not want to just throw exception, but rather print to screen.
	/// Too bad ParseResult didn't allow a pre-parsing, in which we could add to its error collection.
	/// But honestly, since we have to handle printing errors out anyways, ... maybe not much loss there.
	/// </param>
	/// <remarks>
	/// FUTURE WORK!
	/// Consider: `public int Age { get; set; } = 22;` -- a property with default set on the property itself.
	/// Currently we aren't detecting this, and just are overwriting I think with default. ...
	/// MAYBE we should at INIT time, for props w/out a DefVal already, make an instance of the class and
	/// get any of these pre-set values: if exist, set to DefVal on CmdProp. THAT way, the command line would also
	/// have knowledge of that default value and display to user! Pry wouldn't be too hard to do! Super cool EH??!!
	/// </remarks>
	public static bool TrySetValue(ParseResult parseRes, CmdProp p, object model, out string errorMsg)
	{
		errorMsg = null;
		string name = p.Attr.Name;
		Type propTyp = p.PType;

		if(p.IsNumberArray) {
			(bool success, object arr, errorMsg) = GetNumArrVal(parseRes, name, p.NumberArrayItemType, required: p.Attr.Required);
			if(success)
				return setPropVal(arr);
			return false;
		}

		object value = GetValueFromArgName(parseRes, name, p.PType);

		bool hasDefVal = p.DefVal != null;
		if(!hasDefVal)
			return setPropVal(value);

		// --- There's a number of scenarios to deal with IF has a DefVal ---
		// Has to handle ref type vs value type, nullable value type, etc
		// Meanwhile, the following code ***seriously needs clarified!***
		// It's basically working, but I have low confidence until code is much clearer
		// in intent and precise scenarios covered
		// *TODO*

		// THE MAIN ISSUE is, not wanting to overwrite the property value if it's just a default
		// So as to allow properties with a pre-set value, to retain that value if no input changed
		// but below is not completely covering things, particularly in the ELSE...
		// TODO

		if(value == null || value.Equals(p.DefaultOfTVal)) {
			// --- IF NULL or DEFAULT ---
			if(!p.IsNonNullableValueTypeAndValEqualsDefaultTButNotDefValue(value)) { // || (aliases?.Count ?? 0) < 1) {
				value = p.DefVal;
			}
			else {
				// NOT good enough, why just checking aliases?! Goal is to see if token was in input

				var aliases = p.Opt.Aliases;

				int tknMatchCount = parseRes.Tokens.Count(t =>
					t.Type == TokenType.Option &&
					aliases.Any(a => string.Equals(a, t.Value, StringComparison.OrdinalIgnoreCase)));

				if(tknMatchCount < 1) // if(tkn == null) see bug below can't test!
					value = p.DefVal;  // token was NOT in input, so DO use DefVal
			}
		}

		return setPropVal(value);

		bool setPropVal(object val)
		{
			p.Prop.SetValue(model, val);
			return true;
		}
	}


	public static (bool success, object arr, string err) GetNumArrVal(ParseResult pr, string name, Type numType, bool required)
	{
		SymbolResult sr = pr.GetResult(name);

		var tokens = sr?.Tokens;
		if(tokens == null || tokens.Count != 1) {
			if(!required)
				return (true, null, null);
			return (false, null, $"Error: Option '{name}' is required."); // matches curr System.CommandLine style
		}

		Token tkn = tokens[0];
		string value = tkn.Value?.Trim();

		return numType switch {
			Type t when t == typeof(int) => ret(ArgParser.TryParseNumberArray(value, out int[] intArr), intArr),
			Type t when t == typeof(long) => ret(ArgParser.TryParseNumberArray(value, out long[] longArr), longArr),
			Type t when t == typeof(double) => ret(ArgParser.TryParseNumberArray(value, out double[] doubleArr), doubleArr),
			Type t when t == typeof(decimal) => ret(ArgParser.TryParseNumberArray(value, out decimal[] decimalArr), decimalArr),
			_ => (false, null, $"Error: Unsupported numeric array type '{numType.Name}' for argument '{name}'."),
		};

		(bool success, object arr, string err) ret(bool succ, object arrRes) => succ
			? (true, arrRes, null)
			: (false, null, $"Error: Unable to parse numeric array argument '{name}'"); // with value '{value}'
	}

	static object GetValueFromArgName(ParseResult parseRes, string name, Type propTyp)
	{
		// >> get value via attr-name
		// >> get value via Option<T> / Argument<T> ... I quit because getting typeof arg added pain
		MethodInfo method = (typeof(ParseResult)).GetMethod("GetValue", [typeof(string)]).MakeGenericMethod(propTyp);
		object value = method.Invoke(parseRes, [name]);
		return value;
	}
}


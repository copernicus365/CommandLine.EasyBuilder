using System.CommandLine;
using System.CommandLine.Parsing;
using System.Reflection;

namespace CommandLine.EasyBuilder.Internal;

internal class SetPropValue
{
	/// <summary>
	/// Critical method. Used internally by AutoAttributesBinder to perform the
	/// ultimate binding and reading of types / input into custom class.
	/// 
	/// </summary>
	/// <param name="parseRes"></param>
	/// <param name="p"></param>
	/// <param name="item"></param>
	/// <remarks>
	/// FUTURE WORK! --> We need to DETECT scenarios where a property can have a default
	/// set directly on the model class property, and was not input. This is different from
	/// default value, as that has the major benefit of telling the user. HOWEVER, it would be
	/// great to allow our model / class based system **TO NOT OVERWRITE** a property value
	/// set directly on the class (eg `public int Age { get; set; } = 22;`). CURRENTLY we walk
	/// through each attribute property and set the values. Would be great to allow for above scenario
	/// without overwriting above. ... hmmm ... could pry do this AT INIT time, create an instance of
	/// model, and detect any that have a set value, and save that on CmdProp with `HasPreSetValue` or something.
	/// THEN: if input value is default(TProp), 1 last check: to validate it wasn't actually input as default...
	/// (that's difficult tho)
	/// </remarks>
	public static void SetVal(ParseResult parseRes, CmdProp p, object item)
	{
		string name = p.attr.Name;
		Type propTyp = p.type;

		bool _TEST = false;
		if(_TEST)
			TEMP_TEST(parseRes);

		if(p.IsNumberArray) {
			object val = getNumArrVal(parseRes, name, p.NumberArrayItemType);
			setPropVal(val);
			return;
		}

		object value = GetValueFromArgName(parseRes, name, propTyp);

		if(value != null && propTyp.IsValueType && p.isNullable) {
		}

		bool hasDefVal = p.attr.DefVal != null;
		if(!hasDefVal) {
		}
		else if(value == null || value.Equals(p.defaultOfTVal)) {
			// --- IF NULL or DEFAULT ---
			var aliases = p.option.Aliases;

			if(!p.IsNonNullableValueTypeAndValEqualsDefaultTButNotDefValue(value)) { // || (aliases?.Count ?? 0) < 1) {
				value = p.attr.DefVal;
			}
			else {
				// NOTE #1
				int tknMatchCount = parseRes.Tokens.Count(t =>
					t.Type == TokenType.Option &&
					aliases.Any(a => string.Equals(a, t.Value, StringComparison.OrdinalIgnoreCase)));
				if(tknMatchCount < 1) // if(tkn == null) see bug below can't test!
					value = p.attr.DefVal;  // token was NOT in input, so DO use DefVal
			}
		}

		setPropVal(value);

		void setPropVal(object val)
			=> p.pi.SetValue(item, val);
	}

	static object getNumArrVal(ParseResult pr, string name, Type numType)
	{
		SymbolResult sr = pr.GetResult(name);

		var tokens = sr?.Tokens;

		if(tokens == null || tokens.Count != 1)
			return null;

		Token tkn = tokens[0];
		string value = tkn.Value?.Trim();

		object arr = numType switch {
			Type t when t == typeof(int) => ArgParser.ParseNumberArray<int>(value),
			Type t when t == typeof(long) => ArgParser.ParseNumberArray<long>(value),
			Type t when t == typeof(double) => ArgParser.ParseNumberArray<double>(value),
			Type t when t == typeof(decimal) => ArgParser.ParseNumberArray<decimal>(value),
			_ => throw new ArgumentOutOfRangeException(nameof(numType))
		};

		return arr;
	}

	static object GetValueFromArgName(ParseResult parseRes, string name, Type propTyp)
	{
		// >> get value via attr-name
		// >> get value via Option<T> / Argument<T> ... I quit because getting typeof arg added pain
		MethodInfo method = (typeof(ParseResult)).GetMethod("GetValue", [typeof(string)]).MakeGenericMethod(propTyp);
		object value = method.Invoke(parseRes, [name]);
		return value;
	}

	static void TEMP_TEST(ParseResult pr)
	{
		string val1 = pr.GetValue<string>("--name");
		double[] val2 = pr.GetValue<double[]>("--delays");
		string val2_str = pr.GetValue<string>("--delays");
	}
}

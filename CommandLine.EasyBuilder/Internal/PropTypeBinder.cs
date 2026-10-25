using System.CommandLine;

namespace CommandLine.EasyBuilder.Internal;

/// <summary>
/// What EasyBuilder does with a property's type, without reflection: makes its <see cref="Option{T}"/>
/// or <see cref="Argument{T}"/>, sets its default, reads its value, and knows the type's default.
/// One binder per property type, from <see cref="PropTypeBinders"/>.
/// </summary>
public interface IPropTypeBinder
{
	/// <summary>Makes an <see cref="Option{T}"/> of the binder's type.</summary>
	Option CreateOption(string name);

	/// <summary>Makes an <see cref="Argument{T}"/> of the binder's type.</summary>
	Argument CreateArgument(string name);

	/// <summary>
	/// Sets the option's or argument's <c>DefaultValueFactory</c> to <paramref name="value"/>, cast to
	/// the binder's type here, once, when the command tree is built.
	/// </summary>
	void SetDefault(Symbol symbol, object value);

	/// <summary>The option's or argument's value in <paramref name="parseResult"/>: as given, or its default.</summary>
	object GetValue(ParseResult parseResult, Symbol symbol);

	/// <summary>The binder's type's default value, boxed: null for a reference or nullable type.</summary>
	object DefaultOfT { get; }
}

/// <summary>
/// The binder for property type <typeparamref name="T"/>. Wherever it's named (the built-in table, an
/// app's <see cref="AutoBuilder.RegisterTypes"/> step, or the reflection fallback while dynamic code is
/// on), <typeparamref name="T"/> is closed, so a native AOT build gets the exact <see cref="Option{T}"/>,
/// <see cref="Argument{T}"/> and <c>GetValue&lt;T&gt;</c> code it needs.
/// </summary>
public sealed class PropTypeBinder<T> : IPropTypeBinder
{
	public Option CreateOption(string name) => new Option<T>(name);

	public Argument CreateArgument(string name) => new Argument<T>(name);

	public void SetDefault(Symbol symbol, object value)
	{
		T typed = (T)value;

		if(symbol is Option<T> opt)
			opt.DefaultValueFactory = _ => typed;
		else
			((Argument<T>)symbol).DefaultValueFactory = _ => typed;
	}

	public object GetValue(ParseResult parseResult, Symbol symbol) => symbol is Option<T> opt
		? parseResult.GetValue(opt)
		: parseResult.GetValue((Argument<T>)symbol);

	public object DefaultOfT => default(T);
}

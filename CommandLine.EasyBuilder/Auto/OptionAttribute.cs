namespace CommandLine.EasyBuilder.Auto;

public interface IControl { }

public class OptionAttribute : BaseValueDescriptorAttribute { }

public class ArgumentAttribute : BaseValueDescriptorAttribute { }

public class ControlAttribute : BaseValueDescriptorAttribute //, IControl
{ }

public class BaseValueDescriptorAttribute : Attribute
{
	public string Name { get; set; }

	public string Alias { get; set; }

	public string Description { get; set; }

	public object DefVal { get; set; }

	public bool Required { get; set; }

	public string Alias2 { get; set; }
}

//public class OptionAttribute<T> : OptionAttribute
//{
//	public T DefaultValue { get; set; }
//	public ParseArgument<T> Parse3 { get; set; }
//	public Func<ArgumentResult, double[]> Parse { get; set; }
//}

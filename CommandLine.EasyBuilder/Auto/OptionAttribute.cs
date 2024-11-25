namespace CommandLine.EasyBuilder.Auto;

public interface IControl { }

public class OptionAttribute : BaseValueDescriptorAttribute { }

public class ArgumentAttribute : BaseValueDescriptorAttribute { }

public class ControlAttribute : BaseValueDescriptorAttribute { } //, IControl


public class BaseValueDescriptorAttribute : Attribute
{
	public string Name { get; set; }

	public string Alias { get; set; }

	public string Description { get; set; }

	public object DefVal { get; set; }

	public bool Required { get; set; }

	public string Alias2 { get; set; }
}

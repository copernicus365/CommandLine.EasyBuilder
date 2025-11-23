namespace CommandLine.EasyBuilder;

public static class BuilderDI
{
	public static Func<Type, object> ModelInstanceGetter { get; set; }
}

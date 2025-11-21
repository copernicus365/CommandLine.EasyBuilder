using Microsoft.Extensions.DependencyInjection;

public static class EasyBuilderDI
{
	public static IServiceProvider SvcProvider { get; set; }

	public static void SetEasyBuilderDI(this IServiceProvider serviceProvider)
	{
		SvcProvider = serviceProvider;
		CommandLine.EasyBuilder.Internal.CmdModelInfo.GetModelWithDIInstance =
			type => ActivatorUtilities.CreateInstance(SvcProvider, type);
	}
}

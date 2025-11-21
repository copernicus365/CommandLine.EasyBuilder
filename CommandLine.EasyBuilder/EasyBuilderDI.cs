//// FOR DI, add this to your own code. We can't add here without adding a dependency on `Microsoft.Extensions.Hosting`
//// Then at startup just call: `host.Services.SetEasyBuilderDI();` (where `Services` is a `IServiceProvider`)

//using Microsoft.Extensions.DependencyInjection;

//public static class EasyBuilderDI
//{
//	public static IServiceProvider SvcProvider { get; set; }

//	public static void SetEasyBuilderDI(this IServiceProvider serviceProvider)
//	{
//		SvcProvider = serviceProvider;
//		CommandLine.EasyBuilder.Internal.CmdModelInfo.GetModelWithDIInstance =
//			type => ActivatorUtilities.CreateInstance(SvcProvider, type);
//	}
//}
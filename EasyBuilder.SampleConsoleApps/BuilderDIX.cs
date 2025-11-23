// see notes on method below. can add this file to your project...

using CommandLine.EasyBuilder;

using Microsoft.Extensions.DependencyInjection;

public static class BuilderDIX
{
	/// <summary>
	/// For any command models with arguments in the contructor (thinking of DI scenarios),
	/// add this file to your project, and at startup call:
	/// `host.Services.SetEasyBuilderDI();`
	/// <para />
	/// ALTERNATIVE: no need to add this file, make this single line call at startup:
	/// `BuilderDI.ModelInstanceGetter = type => ActivatorUtilities.CreateInstance(host.Services, type);`
	/// </summary>
	/// <param name="serviceProvider"></param>
	public static void SetEasyBuilderDI(this IServiceProvider serviceProvider)
		=> BuilderDI.ModelInstanceGetter = type => ActivatorUtilities.CreateInstance(serviceProvider, type);
}

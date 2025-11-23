using CommandLine.EasyBuilder;

using EasyBuilder.Samples;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

//// FOR TOP-LEVEL (non-Main) way w/Hosting. To use: UNCOMMENT CODE BODY below and rename `Main`

//var builder = Host.CreateApplicationBuilder(args);
//builder.Services.AddSingleton<ICoolSvc, CoolSvc>();

//var host = builder.Build();

////host.Services.SetEasyBuilderDI(); // <-- if BuilderDIX.cs added to project. But one-liner below is all that's needed

//BuilderDI.ModelInstanceGetter = type => ActivatorUtilities.CreateInstance(host.Services, type);

//await HelloYallCmdApp.Run(args);

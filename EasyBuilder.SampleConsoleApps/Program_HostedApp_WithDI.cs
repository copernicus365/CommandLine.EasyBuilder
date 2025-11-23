using CommandLine.EasyBuilder;

using EasyBuilder.Samples;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// FOR TOP-LEVEL (non-Main) way w/Hosting. To use: UNCOMMENT CODE BODY below and rename `Main`

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<ICoolSvc, CoolSvc>();

var host = builder.Build();

BuilderDI.ModelInstanceGetter = type => ActivatorUtilities.CreateInstance(host.Services, type);
// or if BuilderDIX.cs added to proj: //host.Services.SetEasyBuilderDI();

await HelloYallCmdApp.Run(args);

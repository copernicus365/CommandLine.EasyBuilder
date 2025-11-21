using EasyBuilder.Samples.WithServices;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// UNCOMMENT below to have it run instead of Main. Also: canNOT have a namespace here

var builder = Host.CreateApplicationBuilder(args); // 1. Create the builder and register services (same as before)
builder.Services.AddSingleton<ICoolSvc, CoolSvc>();

var host = builder.Build(); // 2. Build the host (this is required – creates the DI container, logger factory, etc.)


host.Services.SetEasyBuilderDI();

// OR!! a SINGLE line, with no using or added file: but it's an ugly line, still ... >>>
//CommandLine.EasyBuilder.Internal.CmdModelInfo.GetModelWithDIInstance = type => ActivatorUtilities.CreateInstance(host.Services, type);


await HelloYallCmdApp.Run(args);

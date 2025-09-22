using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeachingRecordSystem.Worker;
using TeachingRecordSystem.Worker.Infrastructure.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.ConfigureContainer(new DefaultServiceProviderFactory(new ServiceProviderOptions()
{
    ValidateOnBuild = true,
    ValidateScopes = true
}));

if (builder.Environment.IsProduction())
{
    builder.Configuration.AddAksConfiguration();
}

builder.ConfigureLogging();

builder
    .AddCoreServices()
    .AddWorkerServices();

var host = builder.Build();

await host.RunAsync();

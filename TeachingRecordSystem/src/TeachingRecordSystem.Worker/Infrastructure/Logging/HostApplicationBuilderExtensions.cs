using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace TeachingRecordSystem.Worker.Infrastructure.Logging;

public static class HostApplicationBuilderExtensions
{
    public static HostApplicationBuilder ConfigureLogging(this HostApplicationBuilder builder)
    {
        if (builder.Environment.IsProduction())
        {
            SentrySdk.Init(options =>
            {
                options.Dsn = builder.Configuration.GetRequiredValue("Sentry:Dsn");
                options.IsGlobalModeEnabled = true;
            });
        }

        // We want all logging to go through Serilog so that our filters are always applied
        builder.Logging.ClearProviders();

        builder.Services.AddSerilog((services, config) => config.ConfigureSerilog(builder.Environment, builder.Configuration, services));

        return builder;
    }
}

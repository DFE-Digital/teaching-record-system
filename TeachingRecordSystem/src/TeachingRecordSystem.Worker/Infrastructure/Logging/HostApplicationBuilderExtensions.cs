using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace TeachingRecordSystem.Worker.Infrastructure.Logging;

public static class HostApplicationBuilderExtensions
{
    public static HostApplicationBuilder ConfigureLogging(this HostApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();

        if (builder.Environment.IsProduction())
        {
            SentrySdk.Init(options =>
            {
                options.Dsn = builder.Configuration.GetRequiredValue("Sentry:Dsn");
                options.IsGlobalModeEnabled = true;
            });
        }

        builder.Services.AddSerilog((services, config) => config.ConfigureSerilog(builder.Environment, builder.Configuration, services));

        return builder;
    }
}

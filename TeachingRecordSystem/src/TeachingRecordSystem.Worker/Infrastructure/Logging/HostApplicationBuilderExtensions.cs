using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sentry;
using Serilog;
using TeachingRecordSystem.Hosting;

namespace TeachingRecordSystem.Worker.Infrastructure.Logging;

public static class HostApplicationBuilderExtensions
{
    public static HostApplicationBuilder ConfigureLogging(this HostApplicationBuilder builder)
    {
        if (builder.Environment.IsProduction())
        {
            SentrySdk.Init(dsn: builder.Configuration.GetRequiredValue("Sentry:Dsn"));
        }

        builder.Services.AddApplicationInsightsTelemetryWorkerService();

        // We want all logging to go through Serilog so that our filters are always applied
        builder.Logging.ClearProviders();

        builder.Services.AddSerilog((services, config) => config.ConfigureSerilog(builder.Environment, builder.Configuration, services));

        return builder;
    }
}

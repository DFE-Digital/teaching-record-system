using Sentry.Extensibility;
using Serilog;
using TeachingRecordSystem.Api.Infrastructure.ApplicationInsights;

namespace TeachingRecordSystem.Api.Infrastructure.Logging;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsProduction())
        {
            builder.WebHost.UseSentry(dsn: builder.Configuration.GetRequiredValue("Sentry:Dsn"));
        }

        builder.Services.AddSingleton<ISentryEventProcessor, RemoveRedactedUrlParametersEventProcessor>();

        builder.Services.AddApplicationInsightsTelemetry()
            .AddApplicationInsightsTelemetryProcessor<RedactedUrlTelemetryProcessor>();

        // We want all logging to go through Serilog so that our filters are always applied
        builder.Logging.ClearProviders();

        builder.Host.UseSerilog((ctx, services, config) => config.ConfigureSerilog(ctx.HostingEnvironment, ctx.Configuration, services));

        return builder;
    }
}

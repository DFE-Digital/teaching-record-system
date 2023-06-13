using Microsoft.ApplicationInsights.Extensibility;
using QualifiedTeachersApi.Infrastructure.ApplicationInsights;
using Sentry.AspNetCore;
using Sentry.Extensibility;
using Serilog;
using Serilog.Formatting.Compact;

namespace QualifiedTeachersApi.Infrastructure.Logging;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder, string? platformEnvironmentName)
    {
        if (builder.Environment.IsProduction())
        {
            builder.WebHost.UseSentry();

            builder.Services.AddSingleton<ISentryEventProcessor, RemoveRedactedUrlParametersEventProcessor>();

            builder.Services.Configure<SentryAspNetCoreOptions>(options =>
            {
                if (!string.IsNullOrEmpty(platformEnvironmentName))
                {
                    options.Environment = platformEnvironmentName;
                }

                var gitSha = builder.Configuration["GitSha"];
                if (!string.IsNullOrEmpty(gitSha))
                {
                    options.Release = gitSha;
                }
            });
        }

        builder.Services.AddApplicationInsightsTelemetry()
            .AddApplicationInsightsTelemetryProcessor<RedactedUrlTelemetryProcessor>();

        // We want all logging to go through Serilog so that our filters are always applied
        builder.Logging.ClearProviders();

        builder.Host.UseSerilog((ctx, services, config) =>
        {
            config
                .ReadFrom.Configuration(ctx.Configuration)
                .Filter.With(new IgnoreSuppressedExceptionsLogEventFilter())
                .WriteTo.ApplicationInsights(services.GetRequiredService<TelemetryConfiguration>(), TelemetryConverter.Traces)
                .WriteTo.Sentry(o =>
                {
                    o.MinimumBreadcrumbLevel = Serilog.Events.LogEventLevel.Debug;
                    o.MinimumEventLevel = Serilog.Events.LogEventLevel.Error;
                });

            if (ctx.HostingEnvironment.IsProduction())
            {
                config.WriteTo.Console(new CompactJsonFormatter());
            }
            else
            {
                config.WriteTo.Console();
            }
        });

        return builder;
    }
}

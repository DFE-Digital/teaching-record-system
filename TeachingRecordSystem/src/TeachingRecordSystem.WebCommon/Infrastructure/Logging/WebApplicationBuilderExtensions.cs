using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sentry.Extensibility;
using Serilog;

namespace TeachingRecordSystem.WebCommon.Infrastructure.Logging;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder, Action<LoggerConfiguration, IServiceProvider>? configureLogger = null)
    {
        if (builder.Environment.IsProduction())
        {
            builder.WebHost.UseSentry(dsn: builder.Configuration.GetRequiredValue("Sentry:Dsn"));
        }

        builder.Services.AddSingleton<ISentryEventProcessor, RemoveRedactedUrlParametersEventProcessor>();

        builder.Services.AddTransient<RemoveRedactedUrlParametersEnricher>();

        // We want all logging to go through Serilog so that our filters are always applied
        builder.Logging.ClearProviders();

        builder.Host.UseSerilog((ctx, services, config) =>
        {
            config.ConfigureSerilog(ctx.HostingEnvironment, ctx.Configuration, services);

            config.Enrich.With(services.GetRequiredService<RemoveRedactedUrlParametersEnricher>());

            configureLogger?.Invoke(config, services);
        });

        return builder;
    }
}

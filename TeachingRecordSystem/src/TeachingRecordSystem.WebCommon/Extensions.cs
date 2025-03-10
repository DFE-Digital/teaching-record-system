using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Prometheus;
using TeachingRecordSystem.Core.Infrastructure.Configuration;
using TeachingRecordSystem.WebCommon.Infrastructure.Logging;

namespace TeachingRecordSystem.WebCommon;

public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(
        this IHostApplicationBuilder builder,
        string dataProtectionBlobName)
    {
        builder.Services.AddHealthChecks();

        builder.AddDatabase();
        builder.AddHangfire();
        builder.AddBackgroundWorkScheduler();
        builder.AddWebhookMessageFactory();

        builder.Services.AddHealthChecks().AddNpgSql(sp => sp.GetRequiredService<NpgsqlDataSource>());
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddSingleton<UrlRedactor>();

        if (builder.Environment.IsProduction())
        {
            builder.Configuration
                .AddJsonEnvironmentVariable("AppConfig")
                .AddJsonEnvironmentVariable("SharedConfig");

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            builder.Services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });

            builder.Services.AddDataProtection()
                .PersistKeysToAzureBlobStorage(
                    builder.Configuration.GetRequiredValue("StorageConnectionString"),
                    builder.Configuration.GetRequiredValue("DataProtectionKeysContainerName"),
                    dataProtectionBlobName);
        }

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        if (app.Environment.IsProduction())
        {
            app.UseForwardedHeaders();
            app.UseHsts();
        }

        app.MapGet("/health", async context =>
        {
            await context.Response.WriteAsync("OK");
        });

        var gitSha = Environment.GetEnvironmentVariable("GIT_SHA");
        if (gitSha is not null)
        {
            app.MapGet("/sha", async context =>
            {
                await context.Response.WriteAsync(gitSha);
            });
        }

        app.UseHealthChecks("/status");

        app.MapMetrics();
        app.UseHttpMetrics();

        return app;
    }
}

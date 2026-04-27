using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api.Endpoints;
using TeachingRecordSystem.Api.Infrastructure.Logging;
using TeachingRecordSystem.Api.Infrastructure.Middleware;
using TeachingRecordSystem.WebCommon;
using TeachingRecordSystem.WebCommon.Infrastructure.Logging;

[assembly: ApiController]

namespace TeachingRecordSystem.Api;

public class Program
{
#pragma warning disable VSTHRD200
    public static async Task Main(string[] args)
#pragma warning restore VSTHRD200
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.UseDefaultServiceProvider(options =>
        {
            options.ValidateOnBuild = true;
            options.ValidateScopes = true;
        });

        builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

        builder.ConfigureLogging((config, services) =>
        {
            config.Enrich.With(ActivatorUtilities.CreateInstance<AddUserIdLogEventEnricher>(services));
        });

        builder
            .AddServiceDefaults(dataProtectionBlobName: "Api", cacheTableName: "api")
            .AddCoreServices()
            .AddApiServices();

        var app = builder.Build();

        app.MapDefaultEndpoints();

        app.UseMiddleware<AssignRequestedVersionMiddleware>();

        app.UseRouting();
        app.UseTransactions();

        app.UseAuthentication();
        app.UseAuthorization();

        if (builder.Environment.IsProduction())
        {
            // Apply rate limiting to authenticated endpoints
            // (i.e. everywhere except health check, status endpoints etc.)
            app.UseWhen(ctx => ctx.User.Identity?.IsAuthenticated == true, x => x.UseRateLimiter());
        }

        app.MapWebhookJwks();

        app.MapControllers();

        if (builder.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }

        await app.RunAsync();
    }
}

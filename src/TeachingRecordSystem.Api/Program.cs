using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
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

        // Webhook messages link to their schema at DocumentRouteTemplate via ce-dataschema, so don't use the default route
        app.MapOpenApi(OpenApiDocumentHelper.DocumentRouteTemplate);
        app.MapScalarApiReference("/docs", options =>
        {
            options.WithOpenApiRoutePattern(OpenApiDocumentHelper.DocumentRouteTemplate);

            foreach (var (majorVersion, minorVersion) in VersionRegistry.GetAllVersions(builder.Configuration).Reverse())
            {
                options.AddDocument(OpenApiDocumentHelper.GetDocumentName(majorVersion, minorVersion));
            }
        });
        app.MapGet("/swagger", () => Results.Redirect("/docs"));
        app.MapGet("/", () => Results.Redirect("/docs"));

        app.MapWebhookJwks();

        app.MapControllers();

        if (builder.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }

        await app.RunAsync();
    }
}

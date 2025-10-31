using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api;
using TeachingRecordSystem.Api.Endpoints;
using TeachingRecordSystem.Api.Infrastructure.Logging;
using TeachingRecordSystem.Api.Infrastructure.Middleware;
using TeachingRecordSystem.WebCommon;
using TeachingRecordSystem.WebCommon.Infrastructure.Logging;

[assembly: ApiController]

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
    .AddServiceDefaults(dataProtectionBlobName: "Api")
    .AddCoreServices()
    .AddApiServices();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseMiddleware<AssignRequestedVersionMiddleware>();

app.UseRouting();
if (!builder.Environment.IsTests())
{
    app.UseTransactions();
}

app.UseAuthentication();
app.UseAuthorization();

if (builder.Environment.IsProduction())
{
    // Apply rate limiting to authenticated endpoints
    // (i.e. everywhere except health check, status endpoints etc.)
    app.UseWhen(ctx => ctx.User.Identity?.IsAuthenticated == true, x => x.UseRateLimiter());
}

app.Use((ctx, next) =>
{
    ctx.Response.Headers.Append("X-Frame-Options", "deny");
    ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    ctx.Response.Headers.Append("X-XSS-Protection", "0");

    return next();
});

app.MapWebhookJwks();

app.MapControllers();

if (builder.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}

app.Run();

public partial class Program;

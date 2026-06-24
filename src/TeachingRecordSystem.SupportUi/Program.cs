using Hangfire;
using Htmx.TagHelpers;
using Joonasw.AspNetCore.SecurityHeaders;
using TeachingRecordSystem.SupportUi.Endpoints;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;
using TeachingRecordSystem.WebCommon.Infrastructure.Logging;
using TeachingRecordSystem.WebCommon.Middleware;

namespace TeachingRecordSystem.SupportUi;

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

        builder.ConfigureLogging();

        builder
            .AddServiceDefaults(dataProtectionBlobName: "SupportUi", cacheTableName: "ui")
            .AddCoreServices()
            .AddSupportUiServices();

        var app = builder.Build();

        app.MapDefaultEndpoints();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseMigrationsEndPoint();
        }
        else if (!app.Environment.IsTests())
        {
            app.UseExceptionHandler("/error");
            app.UseStatusCodePagesWithReExecute("/error", "?code={0}");
        }

        app.UseCsp(csp =>
        {
            csp.ByDefaultAllow
                .FromSelf();

            csp.AllowScripts
                .FromSelf()
                .AddNonce();

            csp.AllowStyles
                .FromSelf()
                .AddNonce();

            // Ensure ASP.NET Core's auto refresh works
            // See https://github.com/dotnet/aspnetcore/issues/33068
            if (builder.Environment.IsDevelopment())
            {
                csp.AllowConnections
                    .ToAnywhere();
            }
        });

        app.UseMiddleware<AppendSecurityResponseHeadersMiddleware>();

        app.MapStaticAssets();

        app.UseRouting();
        app.UseWhen(ctx => !ctx.Request.Path.StartsWithSegments("/_hangfire"), a => a.UseTransactions());

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHtmxAntiforgeryScript();

        app.MapRazorPages().WithStaticAssets();
        app.MapControllers().WithStaticAssets();

        if (!builder.Environment.IsTests() && !builder.Environment.IsEndToEndTests())
        {
            app.MapHangfireDashboardWithAuthorizationPolicy(AuthorizationPolicies.AdminOnly, "/_hangfire");
        }

        app.MapFiles();

        await app.RunAsync();
    }
}

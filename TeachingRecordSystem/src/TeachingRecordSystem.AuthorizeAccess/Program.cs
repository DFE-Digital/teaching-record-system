using Dfe.Analytics.AspNetCore;
using GovUk.Frontend.AspNetCore;
using Joonasw.AspNetCore.SecurityHeaders;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Middleware;
using TeachingRecordSystem.WebCommon.Infrastructure.Logging;
using TeachingRecordSystem.WebCommon.Middleware;

namespace TeachingRecordSystem.AuthorizeAccess;

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
            .AddServiceDefaults(dataProtectionBlobName: "AuthorizeAccess", cacheTableName: "authz")
            .AddCoreServices()
            .AddAuthorizeAccessServices();

        builder.AddTestApp();

        var app = builder.Build();

        app.UseMiddleware<LogSessionUrlsMiddleware>();

        app.MapDefaultEndpoints();

        app.UseWhen(
            context => !context.Request.Path.StartsWithSegments("/oauth2"),
            a =>
            {
                if (app.Environment.IsDevelopment())
                {
                    a.UseDeveloperExceptionPage();
                    a.UseMigrationsEndPoint();
                }
                else if (!app.Environment.IsTests())
                {
                    a.UseExceptionHandler("/error");
                    a.UseStatusCodePagesWithReExecute("/error", "?code={0}");
                }
            });

        app.UseCsp(csp =>
        {
            var pageTemplateHelper = app.Services.GetRequiredService<PageTemplateHelper>();

            csp.ByDefaultAllow
                .FromSelf();

            csp.AllowScripts
                .FromSelf()
                .From(pageTemplateHelper.GetCspScriptHashes())
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

        app.UseStaticFiles();

        if (builder.Environment.IsProduction())
        {
            app.UseDfeAnalytics();
            app.UseMiddleware<AddAnalyticsDataMiddleware>();
        }

        app.UseRouting();
        app.UseTransactions();

        app.UseSession();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages();
        app.MapControllers();

        app.MapGet("/one-login-jwks", async ctx =>
        {
            var options = ctx.RequestServices.GetRequiredService<IOptions<AuthorizeAccessOptions>>();
            var jwks = options.Value.GetOneLoginSigningKeysJwks();
            await ctx.Response.WriteAsJsonAsync(jwks);
        });

        if (builder.Configuration["RootRedirect"] is string rootRedirect)
        {
            app.MapGet("/", ctx =>
            {
                ctx.Response.Redirect(rootRedirect);
                return Task.CompletedTask;
            });
        }

        await app.RunAsync();
    }
}

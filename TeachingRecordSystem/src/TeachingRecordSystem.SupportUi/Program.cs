using Hangfire;
using Joonasw.AspNetCore.SecurityHeaders;
using TeachingRecordSystem.SupportUi;
using TeachingRecordSystem.SupportUi.Endpoints;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;
using TeachingRecordSystem.WebCommon.Infrastructure.Logging;
using TeachingRecordSystem.WebCommon.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseDefaultServiceProvider(options =>
{
    options.ValidateOnBuild = true;
    options.ValidateScopes = true;
});

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder.ConfigureLogging();

builder
    .AddServiceDefaults(dataProtectionBlobName: "SupportUi")
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
    var pageTemplateHelper = app.Services.GetRequiredService<PageTemplateHelper>();

    csp.ByDefaultAllow
        .FromSelf();

    csp.AllowScripts
        .FromSelf()
        .From(pageTemplateHelper.GetCspScriptHashes())
        .AddNonce();

    csp.AllowStyles
        .FromSelf()
        .AllowUnsafeInline();

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

app.UseRouting();
app.UseWhen(ctx => !ctx.Request.Path.StartsWithSegments("/_hangfire") && !builder.Environment.IsTests(), a => a.UseTransactions());

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

if (!builder.Environment.IsTests() && !builder.Environment.IsEndToEndTests())
{
    app.MapHangfireDashboardWithAuthorizationPolicy(AuthorizationPolicies.AdminOnly, "/_hangfire");
}

app.MapFiles();

app.Run();

public partial class Program;

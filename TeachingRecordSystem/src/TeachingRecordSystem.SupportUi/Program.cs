using GovUk.Frontend.AspNetCore;
using Joonasw.AspNetCore.SecurityHeaders;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using TeachingRecordSystem;
using TeachingRecordSystem.Core.Infrastructure.Configuration;
using TeachingRecordSystem.SupportUi;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    builder.Configuration.AddJsonEnvironmentVariable("AppConfig");
}

if (builder.Environment.IsProduction())
{
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
            "SupportUi");
}

builder.Services.AddGovUkFrontend();
builder.Services.AddCsp(nonceByteAmount: 32);

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration, "AzureAd");

builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.Name = "trs-auth";

    options.Events.OnSigningOut = ctx =>
    {
        ctx.Response.Redirect("/signed-out");
        return Task.CompletedTask;
    };
});

builder.Services.AddRazorPages().AddMvcOptions(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services.AddTransient<TrsLinkGenerator>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseForwardedHeaders();
    app.UseHsts();
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

    // Ensure ASP.NET Core's auto refresh works
    // See https://github.com/dotnet/aspnetcore/issues/33068
    if (builder.Environment.IsDevelopment())
    {
        csp.AllowConnections
            .ToAnywhere();
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", async context =>
{
    await context.Response.WriteAsync("OK");
});

app.MapRazorPages();
app.MapControllers();

app.Run();

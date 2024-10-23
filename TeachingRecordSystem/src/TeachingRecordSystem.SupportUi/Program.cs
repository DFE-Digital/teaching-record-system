using GovUk.Frontend.AspNetCore;
using Hangfire;
using Joonasw.AspNetCore.SecurityHeaders;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem;
using TeachingRecordSystem.Core.Infrastructure;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.PersonMatching;
using TeachingRecordSystem.ServiceDefaults;
using TeachingRecordSystem.ServiceDefaults.Infrastructure.Logging;
using TeachingRecordSystem.SupportUi;
using TeachingRecordSystem.SupportUi.Infrastructure;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using TeachingRecordSystem.SupportUi.Infrastructure.FormFlow;
using TeachingRecordSystem.SupportUi.Infrastructure.ModelBinding;
using TeachingRecordSystem.SupportUi.Infrastructure.Redis;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;
using TeachingRecordSystem.SupportUi.Pages;
using TeachingRecordSystem.SupportUi.Services;
using TeachingRecordSystem.SupportUi.TagHelpers;
using TeachingRecordSystem.UiCommon.Filters;
using TeachingRecordSystem.UiCommon.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder.AddServiceDefaults(dataProtectionBlobName: "SupportUi");

builder.ConfigureLogging();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IDistributedCache, DevelopmentFileDistributedCache>();
}

builder.Services.AddGovUkFrontend();
builder.Services.AddCsp(nonceByteAmount: 32);

if (!builder.Environment.IsUnitTests() && !builder.Environment.IsEndToEndTests())
{
    var graphApiScopes = new[] { "User.Read", "User.ReadBasic.All" };

    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration, "AzureAd", cookieScheme: CookieAuthenticationDefaults.AuthenticationScheme)
        .EnableTokenAcquisitionToCallDownstreamApi(initialScopes: graphApiScopes)
        .AddDistributedTokenCaches()
        .AddMicrosoftGraph(defaultScopes: graphApiScopes);

    builder.Services.ConfigureOptions(new AssignUserInfoOnSignIn(OpenIdConnectDefaults.AuthenticationScheme));

    builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "trs-auth";
        options.Cookie.MaxAge = TimeSpan.FromHours(8);

        options.Events.OnSigningOut = ctx =>
        {
            ctx.Response.Redirect("/signed-out");
            return Task.CompletedTask;
        };
    });
}

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(
        AuthorizationPolicies.ChangeRequestManagement,
        policy => policy
            .RequireAuthenticatedUser()
            .RequireRole(UserRoles.Helpdesk, UserRoles.Administrator))
    .AddPolicy(
        AuthorizationPolicies.UserManagement,
        policy => policy
            .RequireAuthenticatedUser()
            .RequireRole(UserRoles.Administrator))
    .AddPolicy(
        AuthorizationPolicies.Hangfire,
        policy => policy
            .RequireAuthenticatedUser()
            .RequireRole(UserRoles.Administrator));

builder.Services
    .AddRazorPages()
    .AddMvcOptions(options =>
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        options.Filters.Add(new AuthorizeFilter(policy));

        options.Filters.Add(new CheckUserExistsFilter());

        options.Filters.Add(new NoCachePageFilter());

        options.ModelBinderProviders.Insert(2, new DateOnlyModelBinderProvider());
    })
    .AddCookieTempDataProvider(options =>
    {
        options.Cookie.Name = "trs-tempdata";
    });

builder.Services.Scan(s => s.FromAssemblyOf<Program>()
    .AddClasses(f => f.AssignableTo<IConfigureFolderConventions>())
    .As<IConfigureOptions<RazorPagesOptions>>()
    .WithTransientLifetime());

builder.Services.AddRedis(builder.Environment, builder.Configuration);

if (!builder.Environment.IsUnitTests() && !builder.Environment.IsEndToEndTests())
{
    var crmConnectionString = $"""
        AuthType=ClientSecret;
        Url={builder.Configuration.GetRequiredValue("CrmUrl")};
        ClientId={builder.Configuration.GetRequiredValue("AzureAd:ClientId")};
        ClientSecret={builder.Configuration.GetRequiredValue("AzureAd:ClientSecret")};
        RequireNewInstance=true
        """;

    var serviceClient = new ServiceClient(crmConnectionString)
    {
        DisableCrossThreadSafeties = true,
        EnableAffinityCookie = true,
        MaxRetryCount = 2,
        RetryPauseTime = TimeSpan.FromSeconds(1)
    };

    builder.Services.AddDefaultServiceClient(ServiceLifetime.Transient, _ => serviceClient.Clone());

    builder.Services.AddHealthChecks()
        .AddCheck("CRM", () => serviceClient.IsReady ? HealthCheckResult.Healthy() : HealthCheckResult.Degraded());

    if (!builder.Environment.IsDevelopment())
    {
        builder.Services.AddStartupTask<ReferenceDataCache>();
    }
}

builder.AddBlobStorage();

builder.Services
    .AddTrsBaseServices()
    .AddPersonMatching()
    .AddSupportUiServices(builder.Configuration, builder.Environment)
    .AddFormFlow()
    .AddFormFlowJourneyDescriptors(typeof(Program).Assembly)
    .AddFileService()
    .AddTransient<TrsLinkGenerator>()
    .AddSingleton<FeatureProvider>()
    .AddTransient<ICurrentUserIdProvider, HttpContextCurrentUserIdProvider>()
    .AddTransient<CheckMandatoryQualificationExistsFilter>()
    .AddTransient<CheckAlertExistsFilter>()
    .AddTransient<CheckUserExistsFilter>()
    .AddTransient<RequireClosedAlertFilter>()
    .AddTransient<RequireOpenAlertFilter>()
    .AddSingleton<ReferenceDataCache>()
    .AddSingleton<SanctionTextLookup>()
    .AddSingleton<ITagHelperInitializer<FormTagHelper>, FormTagHelperInitializer>();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else if (!app.Environment.IsUnitTests())
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

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

if (!builder.Environment.IsUnitTests() && !builder.Environment.IsEndToEndTests())
{
    app.MapHangfireDashboardWithAuthorizationPolicy(AuthorizationPolicies.Hangfire, "/_hangfire");
}

app.Run();

public partial class Program { }

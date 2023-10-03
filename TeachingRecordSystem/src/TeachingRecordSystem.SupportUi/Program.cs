using GovUk.Frontend.AspNetCore;
using Joonasw.AspNetCore.SecurityHeaders;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Identity.Web;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Infrastructure.Configuration;
using TeachingRecordSystem.SupportUi;
using TeachingRecordSystem.SupportUi.Infrastructure;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using TeachingRecordSystem.SupportUi.Infrastructure.ModelBinding;
using TeachingRecordSystem.SupportUi.Infrastructure.Redis;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;
using TeachingRecordSystem.SupportUi.Services;
using TeachingRecordSystem.SupportUi.TagHelpers;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

if (builder.Environment.IsProduction())
{
    builder.Configuration.AddJsonEnvironmentVariable("AppConfig");
}

var pgConnectionString = builder.Configuration.GetRequiredValue("ConnectionStrings:DefaultConnection");

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
        .AddMicrosoftIdentityWebApp(builder.Configuration, "AzureAd")
        .EnableTokenAcquisitionToCallDownstreamApi(initialScopes: graphApiScopes)
        .AddDistributedTokenCaches()
        .AddMicrosoftGraph(defaultScopes: graphApiScopes);

    builder.Services.ConfigureOptions(new AssignUserInfoOnSignIn(OpenIdConnectDefaults.AuthenticationScheme));

    builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "trs-auth";

        options.Events.OnSigningOut = ctx =>
        {
            ctx.Response.Redirect("/signed-out");
            return Task.CompletedTask;
        };
    });
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AuthorizationPolicies.CaseManagement,
        policy => policy
            .RequireAuthenticatedUser()
            .RequireRole(UserRoles.Helpdesk, UserRoles.Administrator));
});

builder.Services
    .AddRazorPages(options =>
    {
        options.Conventions.AddFolderApplicationModelConvention(
            "/Persons/PersonDetail",
            model =>
            {
                model.Filters.Add(new CheckPersonExistsFilter());
            });
    })
    .AddMvcOptions(options =>
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        options.Filters.Add(new AuthorizeFilter(policy));

        options.Filters.Add(new CheckUserExistsFilter());

        options.ModelBinderProviders.Insert(2, new DateOnlyModelBinderProvider());
    })
    .AddCookieTempDataProvider(options =>
    {
        options.Cookie.Name = "trs-tempdata";
    });

var healthCheckBuilder = builder.Services.AddHealthChecks()
    .AddNpgSql(pgConnectionString);

builder.Services.AddDbContext<TrsDbContext>(
    options => TrsDbContext.ConfigureOptions(options, pgConnectionString),
    contextLifetime: ServiceLifetime.Transient,
    optionsLifetime: ServiceLifetime.Singleton);

builder.Services.AddDbContextFactory<TrsDbContext>(options => TrsDbContext.ConfigureOptions(options, pgConnectionString));

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

builder.Services.AddRedis(builder.Environment, builder.Configuration, healthCheckBuilder);

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

    builder.Services
        .AddTransient<ServiceClient>(sp =>
        {
            var sc = serviceClient.Clone();

            var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                sc.CallerId = httpContext.User.GetCrmUserId();
            }

            return sc;
        })
        .AddTransient<IOrganizationServiceAsync>(sp => sp.GetRequiredService<ServiceClient>());

    healthCheckBuilder.AddCheck("CRM", () => serviceClient.IsReady ? HealthCheckResult.Healthy() : HealthCheckResult.Degraded());
}

builder.Services
    .AddTransient<TrsLinkGenerator>()
    .AddTransient<CheckUserExistsFilter>()
    .AddSingleton<IClock, Clock>()
    .AddSupportUiServices(builder.Configuration, builder.Environment)
    .AddSingleton<ReferenceDataCache>()
    .AddSingleton<ITagHelperInitializer<FormTagHelper>, FormTagHelperInitializer>();

var app = builder.Build();

if (app.Environment.IsProduction())
{
    app.UseForwardedHeaders();
    app.UseHsts();
}

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

    // Ensure ASP.NET Core's auto refresh works
    // See https://github.com/dotnet/aspnetcore/issues/33068
    if (builder.Environment.IsDevelopment())
    {
        csp.AllowConnections
            .ToAnywhere();
    }
});

app.UseHealthChecks("/status");

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

public partial class Program { }

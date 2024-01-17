using GovUk.Frontend.AspNetCore;
using Hangfire;
using Joonasw.AspNetCore.SecurityHeaders;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Identity.Web;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem;
using TeachingRecordSystem.Core.Infrastructure;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.ServiceDefaults;
using TeachingRecordSystem.SupportUi;
using TeachingRecordSystem.SupportUi.Infrastructure;
using TeachingRecordSystem.SupportUi.Infrastructure.Conventions;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using TeachingRecordSystem.SupportUi.Infrastructure.FormFlow;
using TeachingRecordSystem.SupportUi.Infrastructure.Logging;
using TeachingRecordSystem.SupportUi.Infrastructure.ModelBinding;
using TeachingRecordSystem.SupportUi.Infrastructure.Redis;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;
using TeachingRecordSystem.SupportUi.Services;
using TeachingRecordSystem.SupportUi.TagHelpers;

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
    .AddRazorPages(options =>
    {
        options.Conventions.Add(new BindJourneyInstancePropertiesConvention());

        options.Conventions.AddFolderApplicationModelConvention(
            "/Persons/PersonDetail",
            model =>
            {
                model.Filters.Add(new CheckPersonExistsFilterFactory());
            });

        options.Conventions.AddFolderApplicationModelConvention(
            "/Mqs/AddMq",
            model =>
            {
                model.Filters.Add(new CheckPersonExistsFilterFactory());
            });

        options.Conventions.AddFolderApplicationModelConvention(
            "/Mqs/DeleteMq",
            model =>
            {
                model.Filters.Add(new ServiceFilterAttribute<CheckMandatoryQualificationExistsFilter>());
            });

        options.Conventions.AddFolderApplicationModelConvention(
            "/Mqs/EditMq",
            model =>
            {
                model.Filters.Add(new ServiceFilterAttribute<CheckMandatoryQualificationExistsFilter>() { Order = -200 });
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

    builder.Services.AddDefaultServiceClient(
        ServiceLifetime.Transient,
        sp =>
        {
            var sc = serviceClient.Clone();

            var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                sc.CallerId = httpContext.User.GetDqtUserId();
            }

            return sc;
        });

    builder.Services.AddNamedServiceClient(
        "WithoutImpersonation",
        ServiceLifetime.Transient,
        _ => serviceClient.Clone());

    builder.Services.AddHealthChecks()
        .AddCheck("CRM", () => serviceClient.IsReady ? HealthCheckResult.Healthy() : HealthCheckResult.Degraded());
}

builder.Services
    .AddTransient<FormFlow.State.IUserInstanceStateProvider, DbUserInstanceStateProvider>()
    .AddTransient<ICurrentUserIdProvider, HttpContextCurrentUserIdProvider>()
    .AddTransient<CheckMandatoryQualificationExistsFilter>()
    .AddFormFlow(options =>
    {
        options.JourneyRegistry.RegisterJourney(new JourneyDescriptor(
            JourneyNames.AddAlert,
            typeof(TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert.AddAlertState),
            requestDataKeys: ["personId"],
            appendUniqueKey: true));

        options.JourneyRegistry.RegisterJourney(new JourneyDescriptor(
            JourneyNames.CloseAlert,
            typeof(TeachingRecordSystem.SupportUi.Pages.Alerts.CloseAlert.CloseAlertState),
            requestDataKeys: ["alertId"],
            appendUniqueKey: true));

        options.JourneyRegistry.RegisterJourney(new JourneyDescriptor(
            JourneyNames.EditName,
            typeof(TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditName.EditNameState),
            requestDataKeys: ["personId"],
            appendUniqueKey: true));

        options.JourneyRegistry.RegisterJourney(new JourneyDescriptor(
            JourneyNames.EditDateOfBirth,
            typeof(TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDateOfBirth.EditDateOfBirthState),
            requestDataKeys: ["personId"],
            appendUniqueKey: true));

        options.JourneyRegistry.RegisterJourney(new JourneyDescriptor(
            JourneyNames.AddMq,
            typeof(TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq.AddMqState),
            requestDataKeys: ["personId"],
            appendUniqueKey: true));

        options.JourneyRegistry.RegisterJourney(new JourneyDescriptor(
            JourneyNames.EditMqProvider,
            typeof(TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider.EditMqProviderState),
            requestDataKeys: ["qualificationId"],
            appendUniqueKey: true));

        options.JourneyRegistry.RegisterJourney(new JourneyDescriptor(
            JourneyNames.EditMqSpecialism,
            typeof(TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism.EditMqSpecialismState),
            requestDataKeys: ["qualificationId"],
            appendUniqueKey: true));

        options.JourneyRegistry.RegisterJourney(new JourneyDescriptor(
            JourneyNames.EditMqStartDate,
            typeof(TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate.EditMqStartDateState),
            requestDataKeys: ["qualificationId"],
            appendUniqueKey: true));

        options.JourneyRegistry.RegisterJourney(new JourneyDescriptor(
            JourneyNames.EditMqResult,
            typeof(EditMqResultState),
            requestDataKeys: ["qualificationId"],
            appendUniqueKey: true));

        options.JourneyRegistry.RegisterJourney(new JourneyDescriptor(
            JourneyNames.DeleteMq,
            typeof(TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq.DeleteMqState),
            requestDataKeys: ["qualificationId"],
            appendUniqueKey: true));
    });

builder.AddBlobStorage();

builder.Services
    .AddTransient<TrsLinkGenerator>()
    .AddTransient<CheckUserExistsFilter>()
    .AddSingleton<IClock, Clock>()
    .AddSupportUiServices(builder.Configuration, builder.Environment)
    .AddSingleton<ReferenceDataCache>()
    .AddSingleton<SanctionTextLookup>()
    .AddSingleton<ITagHelperInitializer<FormTagHelper>, FormTagHelperInitializer>()
    .AddFileService();

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

    // Ensure ASP.NET Core's auto refresh works
    // See https://github.com/dotnet/aspnetcore/issues/33068
    if (builder.Environment.IsDevelopment())
    {
        csp.AllowConnections
            .ToAnywhere();
    }
});

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

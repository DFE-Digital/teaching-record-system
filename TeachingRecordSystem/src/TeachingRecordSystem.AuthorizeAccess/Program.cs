using System.Security.Cryptography;
using GovUk.Frontend.AspNetCore;
using GovUk.OneLogin.AspNetCore;
using Joonasw.AspNetCore.SecurityHeaders;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.IdentityModel.Tokens;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem;
using TeachingRecordSystem.AuthorizeAccess;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Filters;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.FormFlow;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Logging;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;
using TeachingRecordSystem.AuthorizeAccess.TagHelpers;
using TeachingRecordSystem.Core;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.PersonSearch;
using TeachingRecordSystem.FormFlow;
using TeachingRecordSystem.ServiceDefaults;
using TeachingRecordSystem.SupportUi.Infrastructure.FormFlow;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder.AddServiceDefaults(dataProtectionBlobName: "AuthorizeAccess");

builder.ConfigureLogging();

builder.Services.AddGovUkFrontend();
builder.Services.AddCsp(nonceByteAmount: 32);

var authenticationBuilder = builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = OneLoginDefaults.AuthenticationScheme;

        options.AddScheme(AuthenticationSchemes.FormFlowJourney, scheme =>
        {
            scheme.HandlerType = typeof(FormFlowJourneySignInHandler);
        });

        options.AddScheme(AuthenticationSchemes.MatchToTeachingRecord, scheme =>
        {
            scheme.HandlerType = typeof(MatchToTeachingRecordAuthenticationHandler);
        });
    });

if (!builder.Environment.IsUnitTests() && !builder.Environment.IsEndToEndTests())
{
    authenticationBuilder.AddOneLogin(options =>
    {
        options.SignInScheme = AuthenticationSchemes.FormFlowJourney;

        using (var rsa = RSA.Create())
        {
            var privateKeyPem = builder.Configuration.GetRequiredValue("OneLogin:PrivateKeyPem");
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKeyPem), out _);
            options.ClientAuthenticationCredentials = new SigningCredentials(
                new RsaSecurityKey(rsa.ExportParameters(includePrivateParameters: true)), SecurityAlgorithms.RsaSha256);
        }

        var coreIdentityIssuer = ECDsa.Create();
        var coreIdentityIssuerPem = builder.Configuration.GetRequiredValue("OneLogin:CoreIdentityIssuerPem");
        coreIdentityIssuer.ImportSubjectPublicKeyInfo(Convert.FromBase64String(coreIdentityIssuerPem), out _);
        options.CoreIdentityClaimIssuerSigningKey = new ECDsaSecurityKey(coreIdentityIssuer);
        options.CoreIdentityClaimIssuer = "https://identity.integration.account.gov.uk/";

        options.VectorsOfTrust = @"[""Cl.Cm.P2""]";

        options.Claims.Add(OneLoginClaimTypes.CoreIdentity);

        options.MetadataAddress = "https://oidc.integration.account.gov.uk/.well-known/openid-configuration";
        options.ClientAssertionJwtAudience = "https://oidc.integration.account.gov.uk/token";

        options.ClientId = builder.Configuration.GetRequiredValue("OneLogin:ClientId");
        options.CallbackPath = "/_onelogin/aytq/callback";
        options.SignedOutCallbackPath = "/_onelogin/aytq/logout-callback";
    });
}

builder.Services
    .AddRazorPages()
    .AddMvcOptions(options =>
    {
        options.Filters.Add(new NoCachePageFilter());
    });

if (!builder.Environment.IsUnitTests() && !builder.Environment.IsEndToEndTests())
{
    var crmServiceClient = new ServiceClient(builder.Configuration.GetRequiredValue("ConnectionStrings:Crm"))
    {
        DisableCrossThreadSafeties = true,
        EnableAffinityCookie = true,
        MaxRetryCount = 2,
        RetryPauseTime = TimeSpan.FromSeconds(1)
    };
    builder.Services.AddDefaultServiceClient(ServiceLifetime.Transient, _ => crmServiceClient.Clone());
}

builder.Services
    .AddTrsBaseServices()
    .AddTransient<AuthorizeAccessLinkGenerator, RoutingAuthorizeAccessLinkGenerator>()
    .AddTransient<FormFlowJourneySignInHandler>()
    .AddTransient<MatchToTeachingRecordAuthenticationHandler>()
    .AddFormFlow(options =>
    {
        options.JourneyRegistry.RegisterJourney(SignInJourneyState.JourneyDescriptor);
    })
    .AddSingleton<ICurrentUserIdProvider, DummyCurrentUserIdProvider>()
    .AddTransient<SignInJourneyHelper>()
    .AddSingleton<ITagHelperInitializer<FormTagHelper>, FormTagHelperInitializer>()
    .AddPersonSearch();

builder.Services.AddOptions<AuthorizeAccessOptions>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotations()
    .ValidateOnStart();

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

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();

public partial class Program { }

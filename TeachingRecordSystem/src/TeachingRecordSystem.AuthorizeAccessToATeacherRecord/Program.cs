using System.Security.Cryptography;
using GovUk.Frontend.AspNetCore;
using GovUk.OneLogin.AspNetCore;
using Joonasw.AspNetCore.SecurityHeaders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using TeachingRecordSystem;
using TeachingRecordSystem.AuthorizeAccessToATeacherRecord;
using TeachingRecordSystem.AuthorizeAccessToATeacherRecord.Infrastructure.FormFlow;
using TeachingRecordSystem.AuthorizeAccessToATeacherRecord.Infrastructure.Logging;
using TeachingRecordSystem.AuthorizeAccessToATeacherRecord.Infrastructure.Security;
using TeachingRecordSystem.Core;
using TeachingRecordSystem.FormFlow;
using TeachingRecordSystem.ServiceDefaults;
using TeachingRecordSystem.SupportUi.Infrastructure.FormFlow;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder.AddServiceDefaults(dataProtectionBlobName: "AuthorizeAccess");

builder.ConfigureLogging();

builder.Services.AddGovUkFrontend();
builder.Services.AddCsp(nonceByteAmount: 32);

builder.Services.AddAuthentication(defaultScheme: OneLoginDefaults.AuthenticationScheme)
    .AddOneLogin(options =>
    {
        options.SignInScheme = AuthenticationSchemes.FormFlowJourney;

        var rsa = RSA.Create();
        var privateKeyPem = builder.Configuration.GetRequiredValue("OneLogin:PrivateKeyPem");
        rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKeyPem), out _);
        options.ClientAuthenticationCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);

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

builder.Services.Configure<AuthenticationOptions>(options =>
{
    options.AddScheme(AuthenticationSchemes.FormFlowJourney, scheme =>
    {
        scheme.HandlerType = typeof(FormFlowJourneySignInHandler);
    });

    options.AddScheme(AuthenticationSchemes.MatchToTeachingRecord, scheme =>
    {
        scheme.HandlerType = typeof(MatchToTeachingRecordAuthenticationHandler);
    });
});

builder.Services
    .AddRazorPages();

builder.Services
    .AddSingleton<IClock, Clock>()
    .AddTransient<AuthorizeAccessLinkGenerator>()
    .AddTransient<FormFlowJourneySignInHandler>()
    .AddTransient<MatchToTeachingRecordAuthenticationHandler>()
    .AddFormFlow(options =>
    {
        options.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(SignInJourneyState.JourneyName, typeof(SignInJourneyState), requestDataKeys: [], appendUniqueKey: true));
    })
    .AddSingleton<ICurrentUserIdProvider, DummyCurrentUserIdProvider>()
    .AddSingleton<SignInJourneyHelper>();

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

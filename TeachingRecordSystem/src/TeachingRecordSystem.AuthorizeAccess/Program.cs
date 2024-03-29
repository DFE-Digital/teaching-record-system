using System.Security.Cryptography;
using GovUk.Frontend.AspNetCore;
using GovUk.OneLogin.AspNetCore;
using Joonasw.AspNetCore.SecurityHeaders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem;
using TeachingRecordSystem.AuthorizeAccess;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Filters;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.FormFlow;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Logging;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Oidc;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;
using TeachingRecordSystem.AuthorizeAccess.TagHelpers;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.PersonSearch;
using TeachingRecordSystem.FormFlow;
using TeachingRecordSystem.ServiceDefaults;
using TeachingRecordSystem.SupportUi.Infrastructure.FormFlow;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder.AddServiceDefaults(dataProtectionBlobName: "AuthorizeAccess");

builder.ConfigureLogging();

builder.Services.AddGovUkFrontend();
builder.Services.AddCsp(nonceByteAmount: 32);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = AuthenticationSchemes.MatchToTeachingRecord;

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
    builder.Services
        .TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<OneLoginOptions>, OneLoginPostConfigureOptions>());

    builder.Services
        .Decorate<IAuthenticationSchemeProvider, OneLoginAuthenticationSchemeProvider>()
        .AddSingleton<OneLoginAuthenticationSchemeProvider>(sp => (OneLoginAuthenticationSchemeProvider)sp.GetRequiredService<IAuthenticationSchemeProvider>())
        .AddSingleton<IConfigureOptions<OneLoginOptions>>(sp => sp.GetRequiredService<OneLoginAuthenticationSchemeProvider>())
        .AddSingleton<IHostedService>(sp => sp.GetRequiredService<OneLoginAuthenticationSchemeProvider>());
}

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options
            .UseEntityFrameworkCore()
                .UseDbContext<TrsDbContext>()
                .ReplaceDefaultEntities<Guid>();

        options.ReplaceApplicationManager<ApplicationManager>();
    })
    .AddServer(options =>
    {
        options
            .SetAuthorizationEndpointUris("connect/authorize")
            .SetLogoutEndpointUris("connect/logout")
            .SetTokenEndpointUris("connect/token")
            .SetUserinfoEndpointUris("connect/userinfo");

        // TODO - add teaching record scopes
        options.RegisterScopes(Scopes.Email, Scopes.Profile);

        options.AllowAuthorizationCodeFlow();

        options.DisableAccessTokenEncryption();
        options.SetAccessTokenLifetime(TimeSpan.FromHours(1));

        if (builder.Environment.IsProduction())
        {
            var encryptionKeysConfig = builder.Configuration.GetSection("EncryptionKeys").Get<string[]>() ?? [];
            var signingKeysConfig = builder.Configuration.GetSection("SigningKeys").Get<string[]>() ?? [];

            foreach (var value in encryptionKeysConfig)
            {
                options.AddEncryptionKey(LoadKey(value));
            }

            foreach (var value in signingKeysConfig)
            {
                options.AddSigningKey(LoadKey(value));
            }

            static SecurityKey LoadKey(string configurationValue)
            {
                using var rsa = RSA.Create();
                rsa.FromXmlString(configurationValue);
                return new RsaSecurityKey(rsa.ExportParameters(includePrivateParameters: true));
            }
        }
        else
        {
            options
                .AddDevelopmentEncryptionCertificate()
                .AddDevelopmentSigningCertificate();
        }

        options.UseAspNetCore()
            .EnableAuthorizationEndpointPassthrough()
            .EnableLogoutEndpointPassthrough()
            .EnableTokenEndpointPassthrough()
            .EnableUserinfoEndpointPassthrough()
            .EnableStatusCodePagesIntegration();
    });

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

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();

public partial class Program { }

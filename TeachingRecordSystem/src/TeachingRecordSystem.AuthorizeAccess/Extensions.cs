using System.Security.Cryptography;
using Dfe.Analytics;
using Dfe.Analytics.AspNetCore;
using GovUk.Frontend.AspNetCore;
using GovUk.OneLogin.AspNetCore;
using Joonasw.AspNetCore.SecurityHeaders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Filters;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.FormFlow;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Oidc;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;
using TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;
using TeachingRecordSystem.AuthorizeAccess.TagHelpers;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.GetAnIdentity;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.SupportUi.Infrastructure.FormFlow;
using TeachingRecordSystem.WebCommon.Filters;
using TeachingRecordSystem.WebCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess;

public static class Extensions
{
    public static IHostApplicationBuilder AddAuthorizeAccessServices(this IHostApplicationBuilder builder)
    {
        AddAuthorizeAccessServices(builder.Services, builder.Configuration, builder.Environment);

        return builder;
    }

    public static IServiceCollection AddAuthorizeAccessServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddGovUkFrontend(options =>
        {
            options.Rebrand = true;
            options.DefaultButtonPreventDoubleClick = true;
            options.DefaultFileUploadJavaScriptEnhancements = true;
        });

        services.AddDfeAnalytics()
            .AddAspNetCoreIntegration(options =>
            {
                options.UserIdClaimType = ClaimTypes.Subject;

                options.RequestFilter = ctx =>
                    ctx.Request.Path != "/status" &&
                    ctx.Request.Path != "/health" &&
                    ctx.Features.Any(f => f.Key == typeof(IEndpointFeature));
            });

        services.AddCsp(nonceByteAmount: 32);

        services
            .AddRazorPages()
            .AddMvcOptions(options =>
            {
                options.Filters.Add(new NoCachePageFilter());
                options.Filters.Add(new AssignViewDataFromFormFlowJourneyResultFilterFactory());
            });

        services.AddAuthentication(options =>
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

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options
                    .UseEntityFrameworkCore()
                        .UseDbContext<TrsDbContext>()
                        .ReplaceDefaultEntities<Guid>();

                options.ReplaceApplicationManager<OpenIddictEntityFrameworkCoreApplication<Guid>, ApplicationManager>();
            })
            .AddServer(options =>
            {
                options
                    .SetAuthorizationEndpointUris("oauth2/authorize")
                    .SetEndSessionEndpointUris("oauth2/logout")
                    .SetTokenEndpointUris("oauth2/token")
                    .SetUserInfoEndpointUris("oauth2/userinfo");

                options.SetIssuer(configuration.GetRequiredValue("AuthorizeAccessIssuer"));

                options.RegisterScopes(OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile, CustomScopes.TeachingRecord);

                options.AllowAuthorizationCodeFlow();

                options.DisableAccessTokenEncryption();
                options.SetAccessTokenLifetime(TimeSpan.FromHours(1));

                if (environment.IsProduction())
                {
                    var encryptionKeysConfig = configuration.GetSection("EncryptionKeys").Get<string[]>() ?? [];
                    var signingKeysConfig = configuration.GetSection("SigningKeys").Get<string[]>() ?? [];

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
                    .EnableEndSessionEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough()
                    .EnableStatusCodePagesIntegration();
            });

        services
            .AddTransient<AuthorizeAccessLinkGenerator, RoutingAuthorizeAccessLinkGenerator>()
            .AddTransient<FormFlowJourneySignInHandler>()
            .AddTransient<MatchToTeachingRecordAuthenticationHandler>()
            .AddHttpContextAccessor()
            .AddSingleton<IStartupFilter, FormFlowSessionMiddlewareStartupFilter>()
            .AddFormFlow(options =>
            {
                options.JourneyRegistry.RegisterJourney(SignInJourneyState.JourneyDescriptor);
                options.JourneyRegistry.RegisterJourney(RequestTrnJourneyState.JourneyDescriptor);
            })
            .AddSingleton<ICurrentUserIdProvider, FormFlowSessionCurrentUserIdProvider>()
            .AddTransient<SignInJourneyHelper>()
            .AddSingleton<ITagHelperInitializer<FormTagHelper>, FormTagHelperInitializer>();

        services.AddOptions<AuthorizeAccessOptions>()
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        if (!environment.IsTests() && !environment.IsEndToEndTests())
        {
            services
                .AddDbContext<IdDbContext>(options => options.UseNpgsql(configuration.GetRequiredConnectionString("Id")))
                .AddIdentityApi(configuration);

            services
                .TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<OneLoginOptions>, OneLoginPostConfigureOptions>());

            services
                .Decorate<IAuthenticationSchemeProvider, OneLoginAuthenticationSchemeProvider>()
                .AddSingleton(sp => (OneLoginAuthenticationSchemeProvider)sp.GetRequiredService<IAuthenticationSchemeProvider>())
                .AddSingleton<IConfigureOptions<OneLoginOptions>>(sp => sp.GetRequiredService<OneLoginAuthenticationSchemeProvider>())
                .AddSingleton<IHostedService>(sp => sp.GetRequiredService<OneLoginAuthenticationSchemeProvider>());
        }

        if (environment.IsProduction() || environment.IsDevelopment())
        {
            services.AddNotifyNotificationSender(configuration);
        }
        else
        {
            services.AddSingleton<INotificationSender, NoopNotificationSender>();
        }

        return services;
    }
}

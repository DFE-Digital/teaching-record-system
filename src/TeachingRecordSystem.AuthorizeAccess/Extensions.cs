using System.Security.Cryptography;
using Dfe.Analytics;
using Dfe.Analytics.AspNetCore;
using GovUk.Frontend.AspNetCore;
using GovUk.OneLogin.AspNetCore;
using Joonasw.AspNetCore.SecurityHeaders;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Oidc;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;
using TeachingRecordSystem.AuthorizeAccess.TagHelpers;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.WebCommon.Filters;

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
        if (environment.IsDevelopment())
        {
            services.AddSassCompiler();
        }

        services.AddGovUkFrontend(options =>
        {
            options.DefaultButtonPreventDoubleClick = true;
            options.DefaultFileUploadJavaScriptEnhancements = true;
        });

        // One Login has a one hour timeout on IDV journeys; we need to make sure our session cookies last that long too
        // otherwise callbacks will fail due to the missing journey.
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromHours(2);
            options.Cookie.Name = "sess";
        });

        services.AddGovUkQuestions();

        if (environment.IsDevelopment())
        {
            services.AddDevelopmentJourneyStateStorage();
        }

        services.AddDfeAnalytics()
            .AddAspNetCoreIntegration(options =>
            {
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
                options.Filters.Add(new FluentValidationExceptionFilter());
                options.Filters.Add(new NoCachePageFilter());
            });

        services.Configure<AntiforgeryOptions>(options => options.Cookie.Name = "af");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = AuthenticationSchemes.MatchToTeachingRecord;

            options.AddScheme(AuthenticationSchemes.SignInJourney, scheme =>
            {
                scheme.HandlerType = typeof(JourneySignInHandler);
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

                options.RegisterScopes(OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile, OpenIddictConstants.Scopes.OfflineAccess, CustomScopes.TeachingRecord);

                options.AllowAuthorizationCodeFlow();
                options.AllowRefreshTokenFlow();

                options.DisableAccessTokenEncryption();
                options.SetAccessTokenLifetime(TimeSpan.FromHours(1));
                options.SetRefreshTokenLifetime(TimeSpan.FromDays(10));

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
            .AddTransient<JourneySignInHandler>()
            .AddTransient<MatchToTeachingRecordAuthenticationHandler>()
            .AddHttpContextAccessor()
            .AddSingleton<ITagHelperInitializer<FormTagHelper>, FormTagHelperInitializer>()
            .AddTransient<SignInJourneyCoordinator.LinkHelper>(sp => sp.GetRequiredService<SignInJourneyCoordinator>().Links);

        services.AddOptions<AuthorizeAccessOptions>()
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        if (environment.IsProduction())
        {
            services.AddIdDbContext(configuration);
        }
        else
        {
            services.AddDbContext<IdDbContext>(
                options => options.UseInMemoryDatabase("TeacherAuthId"),
                contextLifetime: ServiceLifetime.Transient);
        }

        if (!environment.IsTests() && !environment.IsEndToEndTests())
        {
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

using GovUk.Frontend.AspNetCore.TagHelpers;
using Joonasw.AspNetCore.SecurityHeaders;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using TeachingRecordSystem.Core.Services.GetAnIdentity;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.SupportUi.Infrastructure;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using TeachingRecordSystem.SupportUi.Infrastructure.FormFlow;
using TeachingRecordSystem.SupportUi.Infrastructure.ModelBinding;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;
using TeachingRecordSystem.SupportUi.Pages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;
using TeachingRecordSystem.SupportUi.Services;
using TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;
using TeachingRecordSystem.SupportUi.TagHelpers;
using TeachingRecordSystem.WebCommon.Filters;
using TeachingRecordSystem.WebCommon.Infrastructure.Redis;

namespace TeachingRecordSystem.SupportUi;

public static class Extensions
{
    public static IHostApplicationBuilder AddSupportUiServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSupportUiServices(builder.Configuration, builder.Environment);

        return builder;
    }

    public static IServiceCollection AddSupportUiServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddGovUkFrontend(options =>
        {
            options.Rebrand = true;
            options.DefaultButtonPreventDoubleClick = true;
            options.DefaultFileUploadJavaScriptEnhancements = true;
        });

        services.AddCsp(nonceByteAmount: 32);

        services
            .AddRazorPages()
            .AddMvcOptions(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                options.Filters.Add(new FluentValidationExceptionFilter());
                options.Filters.Add(new AuthorizeFilter(policy));
                options.Filters.Add(new CheckUserExistsFilter());
                options.Filters.Add(new ServiceFilterAttribute<RedirectWithPersonIdFilter> { Order = RedirectWithPersonIdFilter.Order });
                options.Filters.Add(new NoCachePageFilter());
                options.Filters.Add(new RequireActivePersonFilter());

                options.ModelBinderProviders.Insert(2, new DateOnlyModelBinderProvider());
            })
            .AddCookieTempDataProvider(options =>
            {
                options.Cookie.Name = "trs-tempdata";
            });

        services.Scan(s => s.FromAssemblyOf<Program>()
            .AddClasses(f => f.AssignableTo<IConfigureFolderConventions>())
            .As<IConfigureOptions<RazorPagesOptions>>()
            .WithTransientLifetime());

        services.AddAuthorizationBuilder()
            .AddAdminOnlyPolicies()
            .AddSupportTasksPolicies()
            .AddUserManagementPolicies()
            .AddAlertsPolicies()
            .AddNonPersonOrAlertDataPolicies()
            .AddPersonDataPolicies();

        services
            .AddAzureActiveDirectory(environment)
            .AddFormFlow()
            .AddFormFlowJourneyDescriptors(typeof(Program).Assembly)
            .AddTransient<SupportUiLinkGenerator>()
            .AddTransient<ICurrentUserIdProvider, HttpContextCurrentUserIdProvider>()
            .AddTransient<CheckMandatoryQualificationExistsFilter>()
            .AddTransient<CheckUserExistsFilter>()
            .AddTransient<RequireClosedAlertFilter>()
            .AddTransient<RequireOpenAlertFilter>()
            .AddTransient<RedirectWithPersonIdFilter>()
            .AddSingleton<ITagHelperInitializer<FormTagHelper>, FormTagHelperInitializer>()
            .AddSingleton<ITagHelperInitializer<TextInputTagHelper>, TextInputTagHelperInitializer>()
            .AddScoped<SupportUiFormContext>()
            .AddScoped<SupportUiSortableTableContext>()
            .AddTransient<EvidenceUploadManager>()
            .AddSingleton<PersonChangeableAttributesService>()
            .AddSupportTaskSearchService();

        if (environment.IsProduction())
        {
            services
                .AddStartupTask<ReferenceDataCache>()
                .AddRedis(configuration);
        }

        if (environment.IsDevelopment())
        {
            services.AddSingleton<IDistributedCache, DevelopmentFileDistributedCache>();
        }

        if (!environment.IsTests() && !environment.IsEndToEndTests())
        {
            services
                .AddAzureAdAuthentication(configuration)
                .AddIdentityApi(configuration);
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

    private static IServiceCollection AddAzureAdAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var graphApiScopes = new[] { "User.Read", "User.ReadBasic.All" };

        services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(configuration, "AzureAd", cookieScheme: CookieAuthenticationDefaults.AuthenticationScheme)
            .EnableTokenAcquisitionToCallDownstreamApi(initialScopes: graphApiScopes)
            .AddDistributedTokenCaches()
            .AddMicrosoftGraph(defaultScopes: graphApiScopes);

        services.ConfigureOptions(new AssignUserInfoOnSignIn(OpenIdConnectDefaults.AuthenticationScheme));

        services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.Name = "trs-auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

            options.Events.OnSigningOut = ctx =>
            {
                ctx.Response.Redirect("/signed-out");
                return Task.CompletedTask;
            };

            options.Events.OnRedirectToAccessDenied = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        return services;
    }
}

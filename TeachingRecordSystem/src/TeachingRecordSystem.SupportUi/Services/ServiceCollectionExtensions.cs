using GovUk.Frontend.AspNetCore.TagHelpers;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using TeachingRecordSystem.SupportUi.Infrastructure.FormFlow;
using TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;
using TeachingRecordSystem.SupportUi.TagHelpers;

namespace TeachingRecordSystem.SupportUi.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSupportUiServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services
            .AddAzureActiveDirectory(environment)
            .AddTransient<TrsLinkGenerator>()
            .AddTransient<ICurrentUserIdProvider, HttpContextCurrentUserIdProvider>()
            .AddTransient<CheckMandatoryQualificationExistsFilter>()
            .AddTransient<CheckUserExistsFilter>()
            .AddTransient<RequireClosedAlertFilter>()
            .AddTransient<RequireOpenAlertFilter>()
            .AddSingleton<ReferenceDataCache>()
            .AddSingleton<SanctionTextLookup>()
            .AddSingleton<ITagHelperInitializer<FormTagHelper>, FormTagHelperInitializer>()
            .AddSingleton<ITagHelperInitializer<TextInputTagHelper>, TextInputTagHelperInitializer>()
            .AddFormFlow()
            .AddFormFlowJourneyDescriptors(typeof(Program).Assembly);

        return services;
    }
}

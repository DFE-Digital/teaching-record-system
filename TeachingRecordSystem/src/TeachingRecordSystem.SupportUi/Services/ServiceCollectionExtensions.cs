using TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;

namespace TeachingRecordSystem.SupportUi.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSupportUiServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddAzureActiveDirectory(environment);
        services.AddCrmQueries();

        return services;
    }
}

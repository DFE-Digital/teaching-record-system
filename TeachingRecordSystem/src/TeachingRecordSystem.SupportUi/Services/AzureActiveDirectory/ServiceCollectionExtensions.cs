namespace TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureActiveDirectory(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        if (!environment.IsUnitTests())
        {
            services.AddTransient<IAadUserService, AadUserService>();
        }

        return services;
    }
}

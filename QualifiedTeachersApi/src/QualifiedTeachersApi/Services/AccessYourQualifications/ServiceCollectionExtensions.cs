namespace QualifiedTeachersApi.Services.AccessYourQualifications;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccessYourQualifications(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        if (!environment.IsUnitTests() && !environment.IsEndToEndTests())
        {
            services.AddOptions<AccessYourQualificationsOptions>()
                .Bind(configuration.GetSection("AccessYourQualifications"))
                .ValidateDataAnnotations()
                .ValidateOnStart();
        }

        return services;
    }
}

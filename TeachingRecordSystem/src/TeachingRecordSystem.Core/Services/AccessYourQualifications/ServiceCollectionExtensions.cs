using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TeachingRecordSystem.Core.Services.AccessYourQualifications;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccessYourQualifications(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.Core;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddTrsBaseServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<IClock, Clock>()
            .AddCrmQueries()
            .AddSingleton<PreviousNameHelper>()
            .AddSingleton<IFeatureProvider, ConfigurationFeatureProvider>()
            .AddTrnRequestService();
    }

    public static IServiceCollection AddAccessYourTeachingQualificationsOptions(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        if (!environment.IsUnitTests() && !environment.IsEndToEndTests())
        {
            services.AddOptions<AccessYourTeachingQualificationsOptions>()
                .Bind(configuration.GetSection("AccessYourTeachingQualifications"))
                .ValidateDataAnnotations()
                .ValidateOnStart();
        }

        return services;
    }
}

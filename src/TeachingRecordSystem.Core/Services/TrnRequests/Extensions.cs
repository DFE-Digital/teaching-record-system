using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.TrnRequests;

public static class Extensions
{
    public static IServiceCollection AddTrnRequestService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<TrnRequestService>();

        services.AddOptions<TrnRequestOptions>()
            .Bind(configuration.GetSection("TrnRequests"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}

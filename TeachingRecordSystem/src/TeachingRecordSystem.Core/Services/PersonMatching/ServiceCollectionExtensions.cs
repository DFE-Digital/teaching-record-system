using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.PersonMatching;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersonMatching(this IServiceCollection services)
    {
        services.AddTransient<IPersonMatchingService, PersonMatchingService>();
        return services;
    }
}

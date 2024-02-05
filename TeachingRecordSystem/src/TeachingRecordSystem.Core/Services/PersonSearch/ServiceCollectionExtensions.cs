using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.PersonSearch;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersonSearch(this IServiceCollection services)
    {
        services.AddSingleton<IPersonSearchService, PersonSearchService>();
        return services;
    }
}

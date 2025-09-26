using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.NameSynonyms;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNameSynonyms(this IServiceCollection services)
    {
        services.AddHttpClient<NameSynonymProvider>();
        services.AddSingleton<INameSynonymProvider, NameSynonymProvider>();

        return services;
    }
}

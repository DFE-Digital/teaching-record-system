using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TeachingRecordSystem.Core.Services.NameSynonyms;

public static class ServiceCollectionExtensions
{
    public static IHostApplicationBuilder AddNameSynonyms(this IHostApplicationBuilder builder)
    {
        builder.Services.AddNameSynonyms();

        return builder;
    }

    public static IServiceCollection AddNameSynonyms(this IServiceCollection services)
    {
        services.AddHttpClient<NameSynonymProvider>();
        services.AddSingleton<INameSynonymProvider, NameSynonymProvider>();

        return services;
    }
}

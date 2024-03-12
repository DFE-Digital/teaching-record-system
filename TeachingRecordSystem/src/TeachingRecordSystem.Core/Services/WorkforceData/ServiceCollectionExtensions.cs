using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.WorkforceData;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWorkforceData(this IServiceCollection services)
    {
        services.AddSingleton<ITpsExtractStorageService, BlobStorageTpsExtractStorageService>();
        services.AddSingleton<TpsCsvExtractFileImporter>();
        services.AddSingleton<TpsCsvExtractProcessor>();

        return services;
    }
}

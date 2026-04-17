using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.Services.Establishments.Tps;

namespace TeachingRecordSystem.Core.Services.WorkforceData;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWorkforceData(this IServiceCollection services)
    {
        services.AddSingleton<ITpsExtractStorageService, BlobStorageTpsExtractStorageService>();
        services.AddSingleton<TpsCsvExtractFileImporter>();
        services.AddSingleton<TpsCsvExtractProcessor>();
        services.AddSingleton<TpsEstablishmentRefresher>();

        return services;
    }
}

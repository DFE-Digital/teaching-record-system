using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Services.Establishments.Tps;
using TeachingRecordSystem.Core.Services.WorkforceData.Google;

namespace TeachingRecordSystem.Core.Services.WorkforceData;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWorkforceData(this IServiceCollection services)
    {
        services.AddSingleton<ITpsExtractStorageService, BlobStorageTpsExtractStorageService>();
        services.AddSingleton<TpsCsvExtractFileImporter>();
        services.AddSingleton<TpsCsvExtractProcessor>();
        services.AddSingleton<TpsEstablishmentRefresher>();
        services.AddSingleton<IConfigureOptions<WorkforceDataExportOptions>, WorkforceDataExportConfigureOptions>();
        services.AddSingleton<IStorageClientProvider, OptionsStorageClientProvider>();

        return services;
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace TeachingRecordSystem.Core.Dqt.Services.TrsDataSync;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTrsSyncService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("TrsSyncService:RunService"))
        {
            services.AddOptions<TrsDataSyncServiceOptions>()
                .Bind(configuration.GetSection("TrsSyncService"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<IHostedService, TrsDataSyncService>();

            services.AddServiceClient(
                TrsDataSyncService.CrmClientName,
                sp => new ServiceClient(sp.GetRequiredService<IOptions<TrsDataSyncServiceOptions>>().Value.CrmConnectionString));
        }

        return services;
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Core.Services.TrsDataSync;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddTrsSyncService(this IHostApplicationBuilder builder)
    {
        var runService = builder.Configuration.GetValue<bool>("TrsSyncService:RunService");

        // The RegisterServiceClient option is because jobs that backfill data use the same named ServiceClient
        // so that needs to be around, even if the TrsSyncService itself isn't running.

        if (runService ||
            builder.Configuration.GetValue<bool>("TrsSyncService:RegisterServiceClient"))
        {
            builder.Services.AddOptions<TrsDataSyncServiceOptions>()
                .Bind(builder.Configuration.GetSection("TrsSyncService"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            if (runService)
            {
                builder.Services.AddSingleton<IHostedService, TrsDataSyncService>();
            }

            builder.Services.AddNamedServiceClient(
                TrsDataSyncService.CrmClientName,
                ServiceLifetime.Singleton,
                sp => new ServiceClient(sp.GetRequiredService<IOptions<TrsDataSyncServiceOptions>>().Value.CrmConnectionString));

            builder.Services.AddCrmEntityChangesService(name: TrsDataSyncService.CrmClientName);

            builder.Services.AddSingleton<TrsDataSyncHelper>();
        }

        return builder;
    }
}

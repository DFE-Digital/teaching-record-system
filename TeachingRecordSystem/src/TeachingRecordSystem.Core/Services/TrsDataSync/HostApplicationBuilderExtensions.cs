using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Core.Services.TrsDataSync;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddTrsSyncHelper(this IHostApplicationBuilder builder)
    {
        if (!builder.Environment.IsTests() && !builder.Environment.IsEndToEndTests())
        {
            builder.Services.AddNamedServiceClient(
                TrsDataSyncHelper.CrmClientName,
                ServiceLifetime.Singleton,
                sp => new ServiceClient(sp.GetRequiredService<IConfiguration>()
                    .GetRequiredValue("TrsSyncService:CrmConnectionString")));

            builder.Services.AddSingleton<IAuditRepository, BlobStorageAuditRepository>();
        }

        builder.Services.TryAddSingleton<TrsDataSyncHelper>();

        return builder;
    }
}

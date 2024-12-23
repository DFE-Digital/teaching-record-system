using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Core.Services.TrsDataSync;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddTrsSyncService(this IHostApplicationBuilder builder)
    {
        builder.AddTrsSyncHelper();

        var runService = builder.Configuration.GetValue<bool>("TrsSyncService:RunService");

        builder.Services.AddOptions<TrsDataSyncServiceOptions>()
            .Bind(builder.Configuration.GetSection("TrsSyncService"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        if (runService)
        {
            builder.Services.AddSingleton<IHostedService, TrsDataSyncService>();
        }

        return builder;
    }

    public static IHostApplicationBuilder AddTrsSyncHelper(this IHostApplicationBuilder builder)
    {
        if (!builder.Environment.IsUnitTests() && !builder.Environment.IsEndToEndTests())
        {
            builder.Services.AddNamedServiceClient(
                TrsDataSyncService.CrmClientName,
                ServiceLifetime.Singleton,
                sp => new ServiceClient(sp.GetRequiredService<IConfiguration>()
                    .GetRequiredValue("TrsSyncService:CrmConnectionString")));

            builder.Services.AddCrmEntityChangesService(name: TrsDataSyncService.CrmClientName);

            builder.Services.AddSingleton<IAuditRepository, BlobStorageAuditRepository>();
        }

        builder.Services.TryAddSingleton<TrsDataSyncHelper>();

        return builder;
    }
}

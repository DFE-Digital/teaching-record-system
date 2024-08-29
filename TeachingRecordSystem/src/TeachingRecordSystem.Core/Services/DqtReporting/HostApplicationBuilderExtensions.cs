using Medallion.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Core.Services.DqtReporting;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddDqtReporting(this IHostApplicationBuilder builder)
    {
        if (builder.Configuration.GetValue<bool>("DqtReporting:RunService"))
        {
            builder.Services.AddOptions<DqtReportingOptions>()
                .Bind(builder.Configuration.GetSection("DqtReporting"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services.AddSingleton<IHostedService, DqtReportingService>();

            builder.Services.AddNamedServiceClient(
                DqtReportingService.CrmClientName,
                ServiceLifetime.Singleton,
                sp => new ServiceClient(sp.GetRequiredService<IOptions<DqtReportingOptions>>().Value.CrmConnectionString));

            builder.Services.AddCrmEntityChangesService(name: DqtReportingService.CrmClientName);

            builder.Services.AddStartupTask(async sp =>
            {
                var distributedLockProvider = sp.GetRequiredService<IDistributedLockProvider>();

                await using var @lock = await distributedLockProvider.TryAcquireLockAsync(DistributedLockKeys.DqtReportingMigrations());
                if (@lock is null)
                {
                    return;
                }

                var migrator = new Migrator(
                    connectionString: sp.GetRequiredService<IOptions<DqtReportingOptions>>().Value.ReportingDbConnectionString,
                    logger: sp.GetRequiredService<ILoggerFactory>().CreateLogger<Migrator>());

                migrator.MigrateDb();
            });
        }

        return builder;
    }
}

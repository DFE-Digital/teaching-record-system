using Medallion.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Core.Services.DqtReporting;

public static class Extensions
{
    public static IServiceCollection AddDqtReporting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DqtReportingOptions>()
            .Bind(configuration.GetSection("DqtReporting"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IHostedService, DqtReportingService>();

        services.AddNamedServiceClient(
            DqtReportingService.CrmClientName,
            ServiceLifetime.Singleton,
            sp => new ServiceClient(sp.GetRequiredService<IOptions<DqtReportingOptions>>().Value.CrmConnectionString));

        services.AddCrmEntityChangesService(name: DqtReportingService.CrmClientName);

        services.AddStartupTask(async sp =>
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

        return services;
    }
}

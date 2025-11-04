using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

        services.AddStartupTask(sp =>
        {
            var migrator = new Migrator(
                connectionString: sp.GetRequiredService<IOptions<DqtReportingOptions>>().Value.ReportingDbConnectionString,
                logger: sp.GetRequiredService<ILoggerFactory>().CreateLogger<Migrator>());

            migrator.MigrateDb();

            return Task.CompletedTask;
        });

        return services;
    }
}

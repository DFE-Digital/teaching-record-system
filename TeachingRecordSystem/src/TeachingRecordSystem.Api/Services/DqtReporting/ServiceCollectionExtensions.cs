using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Api.DataStore.Crm;

namespace TeachingRecordSystem.Api.Services.DqtReporting;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDqtReporting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("DqtReporting:RunService"))
        {
            services.AddOptions<DqtReportingOptions>()
                .Bind(configuration.GetSection("DqtReporting"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<IHostedService, DqtReportingService>();

            services.AddApplicationInsightsTelemetry()
                .AddApplicationInsightsTelemetryProcessor<IgnoreDependencyTelemetryProcessor>();

            services.AddServiceClient(
                DqtReportingService.ChangesKey,
                sp => new ServiceClient(sp.GetRequiredService<IOptions<DqtReportingOptions>>().Value.CrmConnectionString));
        }

        return services;
    }
}

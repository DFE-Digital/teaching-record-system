using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace TeachingRecordSystem.Core.Dqt.Services.DqtReporting;

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

            services.AddServiceClient(
                DqtReportingService.ChangesKey,
                sp => new ServiceClient(sp.GetRequiredService<IOptions<DqtReportingOptions>>().Value.CrmConnectionString));
        }

        return services;
    }
}

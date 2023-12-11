using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        }

        return builder;
    }
}

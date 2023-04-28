using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace QualifiedTeachersApi.Services.DqtReporting;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDqtReporting(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
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
        }

        return services;
    }
}

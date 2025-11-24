using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.DqtReporting;
using TeachingRecordSystem.Core.Services.Establishments;
using TeachingRecordSystem.Core.Services.GetAnIdentity;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.Core.Services.PublishApi;
using TeachingRecordSystem.Core.Services.Webhooks;
using TeachingRecordSystem.Core.Services.WorkforceData;

namespace TeachingRecordSystem.Worker;

public static class Extensions
{
    public static IHostApplicationBuilder AddWorkerServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddWorkerServices(builder.Configuration, builder.Environment);

        return builder;
    }

    public static IServiceCollection AddWorkerServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services
            .AddWebhookDeliveryService(configuration)
            .AddHangfireServer()
            .AddWorkforceData()
            .AddMemoryCache();

        if (!environment.IsTests() && !environment.IsEndToEndTests())
        {
            services.AddGias(configuration);
        }

        if (environment.IsProduction())
        {
            services.AddNotifyNotificationSender(configuration);
        }
        else
        {
            services.AddSingleton<INotificationSender, NoopNotificationSender>();
        }

        if (!environment.IsTests() && !environment.IsEndToEndTests())
        {
            services
                .AddIdentityApi(configuration)
                .AddPublishApi(configuration)
                .AddBackgroundJobs(configuration, environment);
        }

        if (configuration.GetValue<bool>("DqtReporting:RunService"))
        {
            services.AddDqtReporting(configuration);
        }

        return services;
    }
}

using Azure.Storage.Blobs;
using Hangfire;
using Medallion.Threading;
using Medallion.Threading.Azure;
using Medallion.Threading.FileSystem;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt;
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
            .AddDistributedLocks(configuration, environment)
            .AddWorkforceData()
            .AddMemoryCache();

        if (configuration.GetValue<string>("ConnectionStrings:Crm") is string crmConnectionString)
        {
            var crmServiceClient = new ServiceClient(crmConnectionString)
            {
                DisableCrossThreadSafeties = true,
                EnableAffinityCookie = true,
                MaxRetryCount = 2,
                RetryPauseTime = TimeSpan.FromSeconds(1)
            };

            services.AddDefaultServiceClient(ServiceLifetime.Transient, _ => crmServiceClient.Clone());
        }

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
                .AddBackgroundJobs(configuration);
        }

        if (configuration.GetValue<bool>("DqtReporting:RunService"))
        {
            services.AddDqtReporting(configuration);
        }

        return services;
    }

    private static IServiceCollection AddDistributedLocks(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.IsProduction())
        {
            var containerName = configuration.GetRequiredValue("DistributedLockContainerName");

            services.AddSingleton<IDistributedLockProvider>(sp =>
            {
                var blobServiceClient = sp.GetRequiredService<BlobServiceClient>();
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
                return new AzureBlobLeaseDistributedSynchronizationProvider(blobContainerClient);
            });
        }
        else
        {
            var lockFileDirectory = Path.Combine(Path.GetTempPath(), "qtlocks");
            services.AddSingleton<IDistributedLockProvider>(new FileDistributedSynchronizationProvider(new DirectoryInfo(lockFileDirectory)));
        }

        return services;
    }
}

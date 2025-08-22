using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Infrastructure;
using TeachingRecordSystem.Core.Infrastructure.Configuration;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.DqtOutbox;
using TeachingRecordSystem.Core.Services.DqtReporting;
using TeachingRecordSystem.Core.Services.Establishments;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.NameSynonyms;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.Core.Services.PublishApi;
using TeachingRecordSystem.Core.Services.TrnGeneration;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.Core.Services.TrsDataSync;
using TeachingRecordSystem.Core.Services.Webhooks;
using TeachingRecordSystem.Core.Services.WorkforceData;
using TeachingRecordSystem.Worker.Infrastructure.Logging;

var builder = Host.CreateApplicationBuilder(args);

if (builder.Environment.IsProduction())
{
    builder.Configuration
        .AddJsonEnvironmentVariable("AppConfig")
        .AddJsonEnvironmentVariable("SharedConfig");
}

builder.ConfigureLogging();

builder
    .AddDatabase()
    .AddBlobStorage()
    .AddDistributedLocks()
    .AddDqtReporting()
    .AddTrsSyncService()
    .AddGias()
    .AddHangfire()
    .AddBackgroundJobs()
    .AddBackgroundWorkScheduler()
    .AddEmail()
    .AddIdentityApi()
    .AddNameSynonyms()
    .AddDqtOutboxMessageProcessorService()
    .AddWebhookDeliveryService()
    .AddWebhookMessageFactory()
    .AddPublishApi()
    .AddTrnRequestService();

var crmServiceClient = new ServiceClient(builder.Configuration.GetRequiredValue("ConnectionStrings:Crm"))
{
    DisableCrossThreadSafeties = true,
    EnableAffinityCookie = true,
    MaxRetryCount = 2,
    RetryPauseTime = TimeSpan.FromSeconds(1)
};
builder.Services.AddDefaultServiceClient(ServiceLifetime.Transient, _ => crmServiceClient.Clone());
AddLegacyDataverseAdapterServices();

builder.Services.AddHangfireServer();

builder.Services
    .AddTrsBaseServices()
    .AddAccessYourTeachingQualificationsOptions(builder.Configuration, builder.Environment)
    .AddWorkforceData()
    .AddMemoryCache()
    .AddFileService();

var host = builder.Build();

await host.RunAsync();

void AddLegacyDataverseAdapterServices()
{
    builder.Services.AddSingleton<ITrnGenerator, DummyTrnGenerationApiClient>();
}

sealed class DummyTrnGenerationApiClient : ITrnGenerator
{
    public Task<string> GenerateTrnAsync() => throw new NotImplementedException();
}

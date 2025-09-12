using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Infrastructure;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.DqtReporting;
using TeachingRecordSystem.Core.Services.Establishments;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.NameSynonyms;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.Core.Services.PersonMatching;
using TeachingRecordSystem.Core.Services.PublishApi;
using TeachingRecordSystem.Core.Services.TrnGeneration;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.Core.Services.TrsDataSync;
using TeachingRecordSystem.Core.Services.Webhooks;
using TeachingRecordSystem.Core.Services.WorkforceData;
using TeachingRecordSystem.Worker.Infrastructure.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.ConfigureContainer(new DefaultServiceProviderFactory(new ServiceProviderOptions()
{
    ValidateOnBuild = true,
    ValidateScopes = true
}));

if (builder.Environment.IsProduction())
{
    builder.Configuration.AddAksConfiguration();
}

builder.ConfigureLogging();

builder
    .AddDatabase()
    .AddBlobStorage()
    .AddDistributedLocks()
    .AddDqtReporting()
    .AddGias()
    .AddHangfire()
    .AddBackgroundJobs()
    .AddBackgroundWorkScheduler()
    .AddEmail()
    .AddIdentityApi()
    .AddNameSynonyms()
    .AddWebhookDeliveryService()
    .AddWebhookMessageFactory()
    .AddPublishApi()
    .AddTrnRequestService()
    .AddTrsSyncHelper();

if (builder.Configuration.GetValue<string>("ConnectionStrings:Crm") is string crmConnectionString)
{
    var crmServiceClient = new ServiceClient(crmConnectionString)
    {
        DisableCrossThreadSafeties = true,
        EnableAffinityCookie = true,
        MaxRetryCount = 2,
        RetryPauseTime = TimeSpan.FromSeconds(1)
    };
    builder.Services.AddDefaultServiceClient(ServiceLifetime.Transient, _ => crmServiceClient.Clone());
}

builder.Services.AddHangfireServer();

builder.Services
    .AddTrsBaseServices()
    .AddAccessYourTeachingQualificationsOptions(builder.Configuration, builder.Environment)
    .AddWorkforceData()
    .AddMemoryCache()
    .AddFileService()
    .AddTrnGeneration()
    .AddPersonMatching();

var host = builder.Build();

await host.RunAsync();

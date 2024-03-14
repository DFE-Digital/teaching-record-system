using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Infrastructure;
using TeachingRecordSystem.Core.Infrastructure.Configuration;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.DqtReporting;
using TeachingRecordSystem.Core.Services.Establishments;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.Core.Services.TrnGenerationApi;
using TeachingRecordSystem.Core.Services.TrsDataSync;
using TeachingRecordSystem.Core.Services.WorkforceData;
using TeachingRecordSystem.Hosting;
using TeachingRecordSystem.Worker.Infrastructure.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<HostOptions>(o => o.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);

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
    .AddIdentityApi();

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
    .AddMemoryCache();

// Filter telemetry emitted by DqtReportingService;
// annoyingly we can't put this into the AddDqtReporting extension method since the method for adding Telemetry Processors
// is different depending on whether you're in a Worker app or Web app :-/
builder.Services.AddApplicationInsightsTelemetryWorkerService()
    .AddApplicationInsightsTelemetryProcessor<IgnoreDependencyTelemetryProcessor>();

var host = builder.Build();

await host.RunAsync();

void AddLegacyDataverseAdapterServices()
{
    builder.Services.AddTransient<IDataverseAdapter, DataverseAdapter>();
    builder.Services.AddSingleton<ITrnGenerationApiClient, DummyTrnGenerationApiClient>();
}

sealed class DummyTrnGenerationApiClient : ITrnGenerationApiClient
{
    public Task<string> GenerateTrn() => throw new NotImplementedException();
}

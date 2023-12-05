using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem;
using TeachingRecordSystem.Core;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt.Services.CrmEntityChanges;
using TeachingRecordSystem.Core.Dqt.Services.DqtReporting;
using TeachingRecordSystem.Core.Dqt.Services.TrsDataSync;
using TeachingRecordSystem.Core.Infrastructure;
using TeachingRecordSystem.Core.Infrastructure.Configuration;
using TeachingRecordSystem.Worker.Infrastructure.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<HostOptions>(o => o.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);

if (builder.Environment.IsProduction())
{
    builder.Configuration.AddJsonEnvironmentVariable("AppConfig");
}

builder.ConfigureLogging();

var pgConnectionString = builder.Configuration.GetRequiredValue("ConnectionStrings:DefaultConnection");

builder.Services.AddDbContext<TrsDbContext>(
    options => TrsDbContext.ConfigureOptions(options, pgConnectionString),
    contextLifetime: ServiceLifetime.Transient,
    optionsLifetime: ServiceLifetime.Singleton);

builder.Services.AddDbContextFactory<TrsDbContext>(options => TrsDbContext.ConfigureOptions(options, pgConnectionString));

builder
    .AddBlobStorage()
    .AddDistributedLocks()
    .AddDqtReporting()
    .AddTrsSyncService();

var crmServiceClient = new ServiceClient(builder.Configuration.GetRequiredValue("ConnectionStrings:Crm"))
{
    DisableCrossThreadSafeties = true,
    EnableAffinityCookie = true,
    MaxRetryCount = 2,
    RetryPauseTime = TimeSpan.FromSeconds(1)
};
builder.Services.AddTransient<IOrganizationServiceAsync>(_ => crmServiceClient.Clone());

builder.Services
    .AddTrsBaseServices()
    .AddCrmEntityChanges();

// Filter telemetry emitted by DqtReportingService;
// annoyingly we can't put this into the AddDqtReporting extension method since the method for adding Telemetry Processors
// is different depending on whether you're in a Worker app or Web app :-/
builder.Services.AddApplicationInsightsTelemetryWorkerService()
    .AddApplicationInsightsTelemetryProcessor<IgnoreDependencyTelemetryProcessor>();

var host = builder.Build();
await host.RunAsync();

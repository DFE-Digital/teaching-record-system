using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.Core.Services.Webhooks;
using TeachingRecordSystem.Core.Tests.ApiSchema;
using TeachingRecordSystem.Core.Tests.Jobs;
using TeachingRecordSystem.Core.Tests.Services.Webhooks;

namespace TeachingRecordSystem.Core.Tests;

public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder) =>
        hostBuilder
            .ConfigureAppConfiguration(builder => builder
                .AddUserSecrets<Startup>(optional: true)
                .AddEnvironmentVariables())
            .ConfigureServices((context, services) =>
            {
                services.AddStartupTask(sp => sp.GetRequiredService<DbHelper>().InitializeAsync());

                services
                    .AddSingleton(DbHelper.Instance)
                    .AddMemoryCache()
                    .AddDatabase(context.Configuration)
                    .AddSingleton<DbFixture>()
                    .AddTestTrnGeneration()
                    .AddSingleton<ReferenceDataCache>()
                    .AddSingleton<TestData>()
                    .AddSingleton<WebhookReceiver>()
                    .AddSingleton<PersonInfoCache>()
                    .AddSingleton<EventMapperFixture>()
                    .AddSingleton<NightlyEmailJobFixture>()
                    .AddSingleton<IClock, TestableClock>()
                    .AddSingleton<WebhookMessageFactory>()
                    .AddSingleton<EventMapperRegistry>()
                    .AddEventPublisher()
                    .AddSupportTaskService();
            });
}

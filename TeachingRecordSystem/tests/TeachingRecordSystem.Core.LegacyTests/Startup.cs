using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.Core.Services.Webhooks;
using TeachingRecordSystem.Core.Tests.Jobs;

namespace TeachingRecordSystem.Core.Tests;

public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder) =>
        hostBuilder
            .ConfigureAppConfiguration(builder => builder.AddConfiguration(TestConfiguration.GetConfiguration()))
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
                    .AddSingleton<PersonInfoCache>()
                    .AddSingleton<NightlyEmailJobFixture>()
                    .AddSingleton<TestableClock>()
                    .AddSingleton<TimeProvider>(sp => sp.GetRequiredService<TestableClock>().TimeProvider)
                    .AddSingleton<WebhookMessageFactory>()
                    .AddSingleton<EventMapperRegistry>()
                    .AddEventPublisher()
                    .AddSupportTaskService();
            });
}

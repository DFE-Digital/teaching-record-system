using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Time.Testing;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.Core.Services.Webhooks;

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
                    .AddSingleton<TimeProvider>(new FakeTimeProvider(new DateTimeOffset(2021, 1, 4, 0, 0, 0, TimeSpan.Zero)))
                    .AddSingleton<WebhookMessageFactory>()
                    .AddSingleton<EventMapperRegistry>()
                    .AddEventPublisher()
                    .AddSupportTaskService();
            });
}

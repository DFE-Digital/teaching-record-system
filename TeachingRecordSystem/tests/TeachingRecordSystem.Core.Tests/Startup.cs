using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                DbHelper.ConfigureDbServices(services, context.Configuration.GetPostgresConnectionString());
                services.AddStartupTask(sp => sp.GetRequiredService<DbHelper>().InitializeAsync());

                services
                    .AddMemoryCache()
                    .AddSingleton<DbFixture>()
                    .AddSingleton<FakeTrnGenerator>()
                    .AddFakeXrm()
                    .AddSingleton<ReferenceDataCache>()
                    .AddSingleton<WebhookReceiver>()
                    .AddSingleton<PersonInfoCache>()
                    .AddSingleton<EventMapperFixture>()
                    .AddSingleton<NightlyEmailJobFixture>()
                    .AddSingleton<IClock, TestableClock>()
                    .AddSingleton<WebhookMessageFactory>()
                    .AddSingleton<EventMapperRegistry>();
            });


}

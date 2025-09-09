using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using TeachingRecordSystem.Core.Dqt;
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
                var pgConnectionString = new NpgsqlConnectionStringBuilder(context.Configuration.GetRequiredConnectionString("DefaultConnection"))
                {
                    // We rely on error details to get the offending duplicate key values in the TrsDataSyncHelper
                    IncludeErrorDetail = true
                }.ConnectionString;

                DbHelper.ConfigureDbServices(services, pgConnectionString);

                services.AddMemoryCache();

                services.AddSingleton<DbFixture>();
                services.AddSingleton<FakeTrnGenerator>();
                services.AddCrmQueries();
                services.AddFakeXrm();
                services.AddSingleton<ReferenceDataCache>();
                services.AddSingleton<WebhookReceiver>();
                services.AddSingleton<PersonInfoCache>();
                services.AddSingleton<EventMapperFixture>();
                services.AddSingleton<NightlyEmailJobFixture>();
                services.AddSingleton<IClock, TestableClock>();
                services.AddSingleton<WebhookMessageFactory>();
                services.AddSingleton<EventMapperRegistry>();
            });


}

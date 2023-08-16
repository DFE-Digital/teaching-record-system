using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
                DbHelper.ConfigureDbServices(services, context.Configuration.GetRequiredConnectionString("DefaultConnection"));

                services.AddSingleton<DbFixture>();
            });
}

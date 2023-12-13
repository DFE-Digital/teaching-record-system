using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Infrastructure.Configuration;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Startup>(optional: true)
            .AddJsonEnvironmentVariable("TEST_CONFIG_JSON")
            .AddEnvironmentVariables()
            .Build();

        DbHelper.ConfigureDbServices(services, configuration.GetRequiredConnectionString("DefaultConnection"));

        var serviceClient = GetCrmServiceClient(configuration);

        services.AddSingleton(serviceClient);
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<CrmClientFixture>();
        services.AddMemoryCache();
        services.AddSingleton<DbFixture>();
        services.AddSingleton<FakeTrnGenerator>();
        services.AddSingleton<ReferenceDataCache>();
        services.AddCrmQueries();

        // This is wrapped up in Task.Run because the ServiceClient constructor can deadlock in some environments (e.g. CI).
        // InitServiceAsync().Result within Microsoft.PowerPlatform.Dataverse.Client.ConnectionService.GetCachedService() looks to be the culprit
        static ServiceClient GetCrmServiceClient(IConfiguration configuration) => Task.Run(() =>
            new ServiceClient(configuration.GetRequiredConnectionString("Crm")))
            .Result;
    }
}

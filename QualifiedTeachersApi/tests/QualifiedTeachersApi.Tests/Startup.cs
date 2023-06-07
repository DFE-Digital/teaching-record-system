using Microsoft.PowerPlatform.Dataverse.Client;
using QualifiedTeachersApi.Tests.DataverseIntegration;
using QualifiedTeachersApi.Tests.Infrastructure;

namespace QualifiedTeachersApi.Tests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var testConfiguration = new TestConfiguration();
        var dbHelper = new DbHelper(testConfiguration.Configuration.GetConnectionString("DefaultConnection") ??
            throw new Exception("Connection string DefaultConnection is missing."));
        var apiFixture = new ApiFixture(testConfiguration, dbHelper);
        var dbFixture = new DbFixture(testConfiguration, dbHelper);

        apiFixture.Initialize().GetAwaiter().GetResult();

        services.AddSingleton(GetCrmServiceClient(testConfiguration.Configuration));
        services.AddSingleton(testConfiguration);
        services.AddSingleton(dbHelper);
        services.AddSingleton(apiFixture);
        services.AddSingleton(dbFixture);
        services.AddSingleton<CrmClientFixture>();
        services.AddMemoryCache();

        // This is wrapped up in Task.Run because the ServiceClient constructor can deadlock in some environments (e.g. CI).
        // InitServiceAsync().Result within Microsoft.PowerPlatform.Dataverse.Client.ConnectionService.GetCachedService() looks to be the culprit
        static ServiceClient GetCrmServiceClient(IConfiguration configuration) => Task.Run(() =>
            new ServiceClient(
                configuration.GetConnectionString("Crm") ?? throw new Exception("Crm connection string is missing.")))
            .Result;
    }
}

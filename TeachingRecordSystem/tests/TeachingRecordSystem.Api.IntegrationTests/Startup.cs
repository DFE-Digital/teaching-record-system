namespace TeachingRecordSystem.Api.IntegrationTests;

public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder)
    {
        var settings = new Dictionary<string, string>
        {
            { "GetAnIdentityApplicationUserId", HostFixture.GetAnIdentityApplicationUserId.ToString() }
        };

        hostBuilder
            .ConfigureHostConfiguration(builder => builder
                .AddUserSecrets<HostFixture>(optional: true)
                .AddEnvironmentVariables()
                .AddInMemoryCollection(settings!))
            .ConfigureServices(services =>
            {
                services.AddSingleton<HostFixture>();
            });
    }
}

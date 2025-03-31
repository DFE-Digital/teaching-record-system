namespace TeachingRecordSystem.Api.Tests;

public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder)
    {
        hostBuilder
            .ConfigureHostConfiguration(builder => builder
                .AddUserSecrets<HostFixture>(optional: true)
                .AddEnvironmentVariables()
                .AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["AllowContactPiiUpdatesFromUserIds:0"] = HostFixture.DefaultApplicationUserId.ToString()
                    }))
            .ConfigureServices(services =>
            {
                services.AddSingleton<HostFixture>();
            });
    }
}

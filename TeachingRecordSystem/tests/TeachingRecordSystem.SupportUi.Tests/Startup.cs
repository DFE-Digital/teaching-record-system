namespace TeachingRecordSystem.SupportUi.Tests;

public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder) =>
        hostBuilder
            .ConfigureHostConfiguration(builder => builder
                .AddUserSecrets<HostFixture>(optional: true)
                .AddEnvironmentVariables())
            .ConfigureServices(services =>
            {
                services.AddSingleton<HostFixture>();
            });
}

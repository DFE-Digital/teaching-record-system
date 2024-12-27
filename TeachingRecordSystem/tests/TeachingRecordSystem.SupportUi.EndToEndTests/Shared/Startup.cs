namespace TeachingRecordSystem.SupportUi.EndToEndTests.Shared;

public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder) =>
        hostBuilder
            .ConfigureHostConfiguration(builder => builder
                .AddUserSecrets<Startup>()
                .AddEnvironmentVariables())
            .ConfigureServices(services =>
            {
                services.AddSingleton<HostFixture>();
                services.AddStartupTask<HostFixture>();
            });
}

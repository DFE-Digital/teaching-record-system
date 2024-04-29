namespace TeachingRecordSystem.AuthorizeAccess.Tests;

public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder) =>
        hostBuilder
            .ConfigureHostConfiguration(builder => builder
                .AddUserSecrets<Startup>(optional: true)
                .AddEnvironmentVariables())
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<HostFixture>();
            });
}

namespace TeachingRecordSystem.Api.Tests;

public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder) =>
        hostBuilder
            .ConfigureHostConfiguration(builder => builder
                .AddUserSecrets<ApiFixture>(optional: true)
                .AddEnvironmentVariables())
            .ConfigureServices(services =>
            {
                services.AddSingleton<ApiFixture>();
            });
}

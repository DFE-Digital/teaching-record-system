using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace TeachingRecordSystem.Api.UnitTests;

public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder)
    {
        hostBuilder
            .ConfigureHostConfiguration(builder => builder
                .AddUserSecrets<Startup>(optional: true)
                .AddEnvironmentVariables())
            .ConfigureServices(services =>
            {

            });
    }
}

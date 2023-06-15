using TeachingRecordSystem.Api.Infrastructure.Configuration;

namespace TeachingRecordSystem.Api.Tests.Infrastructure;

public class TestConfiguration
{
    public IConfiguration Configuration { get; } =
        new ConfigurationBuilder()
            .AddUserSecrets<ApiFixture>(optional: true)
            .AddJsonEnvironmentVariable("TEST_CONFIG_JSON")
            .AddEnvironmentVariables()
            .Build();
}

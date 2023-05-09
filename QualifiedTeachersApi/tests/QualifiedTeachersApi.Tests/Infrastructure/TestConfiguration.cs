using Microsoft.Extensions.Configuration;
using QualifiedTeachersApi.Infrastructure.Configuration;

namespace QualifiedTeachersApi.Tests.Infrastructure;

public class TestConfiguration
{
    public IConfiguration Configuration { get; } =
        new ConfigurationBuilder()
            .AddUserSecrets<ApiFixture>(optional: true)
            .AddJsonEnvironmentVariable("TEST_CONFIG_JSON")
            .AddEnvironmentVariables()
            .Build();
}

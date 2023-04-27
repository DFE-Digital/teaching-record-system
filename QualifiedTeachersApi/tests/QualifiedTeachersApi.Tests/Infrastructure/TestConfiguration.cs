using Microsoft.Extensions.Configuration;

namespace QualifiedTeachersApi.Tests.Infrastructure;

public class TestConfiguration
{
    public IConfiguration Configuration { get; } =
        new ConfigurationBuilder()
            .AddUserSecrets<ApiFixture>(optional: true)
            .AddEnvironmentVariables()
            .Build();
}

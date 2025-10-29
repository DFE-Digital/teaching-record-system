using Microsoft.Extensions.Configuration;

namespace TeachingRecordSystem.TestCommon;

public static class TestConfiguration
{
    public static ConfigurationManager GetConfiguration()
    {
        var configuration = new ConfigurationManager();

        configuration
            .AddUserSecrets("TeachingRecordSystemTests")
            .AddEnvironmentVariables();

        return configuration;
    }
}

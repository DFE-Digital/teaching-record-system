using Microsoft.Extensions.Configuration;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.TestCommon;

public static class TestConfiguration
{
    public static ConfigurationManager GetConfiguration()
    {
        var configuration = new ConfigurationManager();

        configuration
            .AddUserSecrets("TeachingRecordSystemTests")
            .AddEnvironmentVariables();

        var connectionString = configuration.GetConnectionString(TrsDbContext.ConnectionName);
        if (connectionString is null)
        {
            connectionString = DbHelper.GetTestContainersConnectionString();

            configuration.AddInMemoryCollection([
                KeyValuePair.Create($"ConnectionStrings:{TrsDbContext.ConnectionName}", (string?)connectionString),
                KeyValuePair.Create("UseTestContainers", (string?)"true")
            ]);
        }

        return configuration;
    }
}

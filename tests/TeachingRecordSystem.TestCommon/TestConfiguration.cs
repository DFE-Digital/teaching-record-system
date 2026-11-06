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

        var connectionString = TestDatabase.ConnectionString;

        configuration.AddInMemoryCollection([
            KeyValuePair.Create($"ConnectionStrings:{TrsDbContext.ConnectionName}", (string?)connectionString)
        ]);

        // The hosts that the test WebApplicationFactorys create build their own configuration and add environment
        // variables *after* the configuration we hand them via UseConfiguration, so an environment variable would
        // override the value above for those hosts only. Overwriting it here keeps every configuration built in
        // this process pointing at this process's database.
        Environment.SetEnvironmentVariable($"ConnectionStrings__{TrsDbContext.ConnectionName}", connectionString);

        return configuration;
    }
}

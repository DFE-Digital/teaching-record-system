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

        // When UseTestContainers is set the testcontainer's connection string always wins over any configured
        // connection string; otherwise we'd start a container that nothing connects to and run the tests against
        // whatever database happens to be configured in user secrets.
        // Not having a connection string configured at all implies testcontainers.
        if (configuration.GetValue<bool>("UseTestContainers") || connectionString is null)
        {
            connectionString = DbHelper.GetTestContainersConnectionString(DbHelper.GetTestContainersPostgresPort(configuration));

            configuration.AddInMemoryCollection([
                KeyValuePair.Create($"ConnectionStrings:{TrsDbContext.ConnectionName}", (string?)connectionString),
                KeyValuePair.Create("UseTestContainers", (string?)"true")
            ]);

            // The hosts that the test WebApplicationFactorys create build their own configuration and add environment
            // variables *after* the configuration we hand them via UseConfiguration, so an environment variable would
            // override the value above for those hosts only. Overwriting it here keeps every configuration built in
            // this process pointing at the container.
            // Both variables have to be set: setting only the connection string would make a subsequent call here take
            // the other branch, and DbHelper would then never build the container to connect to.
            Environment.SetEnvironmentVariable("UseTestContainers", "true");
            Environment.SetEnvironmentVariable($"ConnectionStrings__{TrsDbContext.ConnectionName}", connectionString);
        }

        return configuration;
    }
}

using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Npgsql;
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
        var useTestContainers = configuration.GetValue<bool>("UseTestContainers") || connectionString is null;
        if (useTestContainers)
        {
            connectionString = DbHelper.GetTestContainersConnectionString(DbHelper.GetTestContainersPostgresPort(configuration));
        }

        // Whatever connection string we end up with supplies the server and the credentials; the database is always one
        // of our own, so that test projects and worktrees sharing a server can't clear each other's data down mid-run.
        connectionString = new NpgsqlConnectionStringBuilder(connectionString) { Database = GetDatabaseName() }.ConnectionString;

        configuration.AddInMemoryCollection([
            KeyValuePair.Create($"ConnectionStrings:{TrsDbContext.ConnectionName}", (string?)connectionString),
            KeyValuePair.Create("UseTestContainers", (string?)(useTestContainers ? "true" : "false"))
        ]);

        // The hosts that the test WebApplicationFactorys create build their own configuration and add environment
        // variables *after* the configuration we hand them via UseConfiguration, so an environment variable would
        // override the values above for those hosts only. Overwriting them here keeps every configuration built in
        // this process pointing at the same database.
        // Both variables have to be set: setting only the connection string would make a subsequent call here take
        // the other branch, and DbHelper would then never build the container to connect to.
        Environment.SetEnvironmentVariable("UseTestContainers", useTestContainers ? "true" : "false");
        Environment.SetEnvironmentVariable($"ConnectionStrings__{TrsDbContext.ConnectionName}", connectionString);

        return configuration;
    }

    // Databases are shared between worktrees whenever they're pointed at the same server, so the name has to cover
    // where the tests are running from as well as which project they're in.
    public static string GetDatabaseName()
    {
        var repositoryHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(TestPaths.RepositoryRoot)))[..8];

        var projectName = TestPaths.TestProjectName
            .Replace("TeachingRecordSystem.", "", StringComparison.Ordinal)
            .Replace('.', '_');

        return $"trs_{repositoryHash}_{projectName}".ToLowerInvariant();
    }
}

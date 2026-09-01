using Npgsql;

namespace TeachingRecordSystem.TestCommon.Database;

// The Postgres instance the template and the pool live on. Whichever server the existing configuration
// points at is reused, so this works against a testcontainer, a local install or a CI service container
// without any extra configuration.
public sealed class TestDatabaseServer(string maintenanceConnectionString)
{
    public static async Task<TestDatabaseServer> EnsureStartedAsync()
    {
        var configuration = TestConfiguration.GetConfiguration();

        // Starts the testcontainer if that's what's configured; a no-op otherwise.
        await DbHelper.Instance.EnsureContainerStartedAsync();

        return new TestDatabaseServer(configuration.GetPostgresConnectionString());
    }

    public string ConnectionStringFor(string database) =>
        new NpgsqlConnectionStringBuilder(maintenanceConnectionString)
        {
            Database = database,
            // Every pooled database gets its own data source. Without a cap, a pool of N databases times
            // Npgsql's default max pool size of 100 exhausts the server's connection limit.
            MaxPoolSize = 8
        }.ConnectionString;

    public async Task ExecuteAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(ConnectionStringFor("postgres"));
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    public async Task<T?> ExecuteScalarAsync<T>(string sql, params (string Name, object Value)[] parameters)
    {
        await using var connection = new NpgsqlConnection(ConnectionStringFor("postgres"));
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        var result = await command.ExecuteScalarAsync();
        return result is null or DBNull ? default : (T)result;
    }
}

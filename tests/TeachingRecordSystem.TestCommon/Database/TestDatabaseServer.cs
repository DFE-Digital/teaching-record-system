using Microsoft.Extensions.Configuration;
using Npgsql;
using Testcontainers.PostgreSql;

namespace TeachingRecordSystem.TestCommon.Database;

// The Postgres instance the template and the pool live on. Whichever server the existing configuration
// points at is reused, so this works against a testcontainer, a local install or a CI service container
// without any extra configuration.
public sealed class TestDatabaseServer(string maintenanceConnectionString)
{
    private const int DefaultTestContainersPostgresPort = 43007;

    private static readonly SemaphoreSlim _containerGate = new(1, 1);
    private static PostgreSqlContainer? _container;

    public static string GetTestContainersConnectionString(int port) =>
        $"Host=localhost;Port={port};Database=trs;Username=postgres;Password=postgres;";

    public static int GetTestContainersPostgresPort(IConfiguration configuration) =>
        configuration.GetValue<int?>("TestContainersPostgresPort") ?? DefaultTestContainersPostgresPort;

    public static async Task<TestDatabaseServer> EnsureStartedAsync()
    {
        var configuration = TestConfiguration.GetConfiguration();

        if (configuration.GetValue<bool>("UseTestContainers"))
        {
            await EnsureContainerStartedAsync(configuration);
        }

        return new TestDatabaseServer(configuration.GetPostgresConnectionString());
    }

    // The container is reused across runs and never stopped from here, so the template it holds survives
    // from one run to the next.
    private static async Task EnsureContainerStartedAsync(IConfiguration configuration)
    {
        await _containerGate.WaitAsync();

        try
        {
            if (_container is not null)
            {
                return;
            }

            var port = GetTestContainersPostgresPort(configuration);

            // Test projects are often started together. Left to themselves, each would find no container to reuse
            // and create its own, and all but the first would fail to bind the port; taking turns means the rest
            // find the first one's container and reuse it.
            await using var startLock = await AcquireStartLockAsync(port);

            var container = new PostgreSqlBuilder("postgres:17")
                .WithDatabase("trs")
                .WithReuse(true)
                .WithPortBinding(port, 5432)
                // Every process keeps a connection pool per database it has leased, and test projects are often
                // run side by side against one container; Postgres' default of 100 connections runs out.
                .WithCommand("-c", "max_connections=500")
                .Build();

            await container.StartAsync();
            _container = container;
        }
        finally
        {
            _containerGate.Release();
        }
    }

    private static async Task<FileStream> AcquireStartLockAsync(int port)
    {
        var lockPath = Path.Combine(Path.GetTempPath(), $"trs-tests-postgres-{port}.lock");

        while (true)
        {
            try
            {
                return new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
        }
    }

    public string ConnectionStringFor(string database) =>
        new NpgsqlConnectionStringBuilder(maintenanceConnectionString)
        {
            Database = database,
            // Every pooled database gets its own data source. Without a cap, a pool of N databases times
            // Npgsql's default max pool size of 100 exhausts the server's connection limit.
            MaxPoolSize = 8
        }.ConnectionString;

    // Advisory locks belong to the session that took them, and a pooled connection is reset (which releases
    // them) when it goes back to the pool, so anything holding a lock needs a connection of its own.
    public async Task<NpgsqlConnection> OpenLockConnectionAsync()
    {
        var connectionString = new NpgsqlConnectionStringBuilder(maintenanceConnectionString)
        {
            Database = "postgres",
            Pooling = false
        }.ConnectionString;

        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        return connection;
    }

    // Keyed on the database's name so that every process on the server agrees on which lock guards it.
    public static Task<bool> TryLockAsync(NpgsqlConnection lockConnection, string databaseName) =>
        ExecuteScalarAsync<bool>(lockConnection, "select pg_try_advisory_lock(hashtextextended(@name, 0))", ("name", databaseName));

    public static Task LockAsync(NpgsqlConnection lockConnection, string lockName) =>
        ExecuteScalarAsync<object>(lockConnection, "select pg_advisory_lock(hashtextextended(@name, 0))", ("name", lockName));

    public static Task LockSharedAsync(NpgsqlConnection lockConnection, string lockName) =>
        ExecuteScalarAsync<object>(lockConnection, "select pg_advisory_lock_shared(hashtextextended(@name, 0))", ("name", lockName));

    public static Task UnlockAsync(NpgsqlConnection lockConnection, string lockName) =>
        ExecuteScalarAsync<object>(lockConnection, "select pg_advisory_unlock(hashtextextended(@name, 0))", ("name", lockName));

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
        return await ExecuteScalarAsync<T>(connection, sql, parameters);
    }

    public static async Task<T?> ExecuteScalarAsync<T>(NpgsqlConnection connection, string sql, params (string Name, object Value)[] parameters)
    {
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

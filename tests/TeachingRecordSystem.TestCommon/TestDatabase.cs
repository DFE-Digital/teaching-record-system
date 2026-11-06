using System.Net;
using System.Security.Cryptography;
using Docker.DotNet;
using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;
using Testcontainers.PostgreSql;

namespace TeachingRecordSystem.TestCommon;

// Gives each test process a database of its own, cloned from a template, on a container shared by the whole machine.
// See docs/test-databases.md.
public static class TestDatabase
{
    private const string ContainerName = "trs-tests-postgres-17";
    private const string DataDirectory = "/var/lib/postgresql/data";
    private const string TemplatePrefix = "trs_template_";
    private const string TemplateBuildName = TemplatePrefix + "build";
    private const int TemplatesToKeep = 5;

    // Advisory locks are keyed on (AdvisoryLockClass, key); key 0 guards templates and the rest are database slots.
    private const int AdvisoryLockClass = 7007;
    private const int TemplateLockKey = 0;

    private static readonly TimeSpan _startTimeout = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan _retryDelay = TimeSpan.FromMilliseconds(250);

    // The connection string is needed by synchronous code, including test fixtures' constructors.
#pragma warning disable VSTHRD002, VSTHRD011
    private static readonly Lazy<Lease> _lease = new(() => Task.Run(AcquireLeaseAsync).GetAwaiter().GetResult());
#pragma warning restore VSTHRD002, VSTHRD011

    public static string ConnectionString => _lease.Value.ConnectionString;

    private static async Task<Lease> AcquireLeaseAsync()
    {
        var serverConnectionString = await StartContainerAsync();

        // A session's advisory locks are released when it ends, so a process that dies can't leave its slot locked.
        // That means this connection has to stay open, and unpooled, until the process exits.
        var lockConnection = await OpenLockConnectionAsync(serverConnectionString);

        await ExecuteAsync(lockConnection, $"select pg_advisory_lock({AdvisoryLockClass}, {TemplateLockKey})");

        try
        {
            var templateName = GetTemplateName();
            await EnsureTemplateAsync(lockConnection, serverConnectionString, templateName);
            var databaseName = await CreateDatabaseInFreeSlotAsync(lockConnection, templateName);

            return new Lease(WithDatabase(serverConnectionString, databaseName), lockConnection);
        }
        finally
        {
            await ExecuteAsync(lockConnection, $"select pg_advisory_unlock({AdvisoryLockClass}, {TemplateLockKey})");
        }
    }

    private static async Task<string> StartContainerAsync()
    {
        var deadline = DateTime.UtcNow + _startTimeout;

        while (true)
        {
            var container = new PostgreSqlBuilder("postgres:17")
                .WithName(ContainerName)
                .WithDatabase("postgres")
                .WithReuse(true)
                .WithPortBinding(PostgreSqlBuilder.PostgreSqlPort, assignRandomHostPort: true)
                // These are appended to the arguments PostgreSqlBuilder adds, which already turn off fsync.
                .WithCommand(
                    "-c", "max_connections=500",
                    "-c", "wal_level=minimal",
                    "-c", "max_wal_senders=0",
                    "-c", "max_wal_size=256MB")
                // Holding the data on tmpfs and removing the container when it stops means nothing outlives it;
                // WithAutoRemove can't be used since Testcontainers rejects it alongside WithReuse.
                .WithCreateParameterModifier(parameters =>
                {
                    parameters.HostConfig!.AutoRemove = true;
                    parameters.HostConfig.Tmpfs = new Dictionary<string, string> { [DataDirectory] = "rw,size=2g" };
                })
                .Build();

            try
            {
                await container.StartAsync();
                return container.GetConnectionString();
            }
            catch (DockerApiException ex) when (ex.StatusCode == HttpStatusCode.Conflict && DateTime.UtcNow < deadline)
            {
                // Another process created the container first; it can be reused once it's running.
                await Task.Delay(_retryDelay);
            }
        }
    }

    private static async Task<NpgsqlConnection> OpenLockConnectionAsync(string serverConnectionString)
    {
        var connectionString = new NpgsqlConnectionStringBuilder(serverConnectionString) { Pooling = false }.ConnectionString;
        var deadline = DateTime.UtcNow + _startTimeout;

        while (true)
        {
            var connection = new NpgsqlConnection(connectionString);

            try
            {
                await connection.OpenAsync();
                return connection;
            }
            catch (NpgsqlException) when (DateTime.UtcNow < deadline)
            {
                // A container reused while another process is starting it may not be accepting connections yet.
                await connection.DisposeAsync();
                await Task.Delay(_retryDelay);
            }
        }
    }

    // Migrations and seed data live in code in these assemblies, so a change to either can change what a template holds.
    private static string GetTemplateName()
    {
        var versions = new[] { typeof(TrsDbContext), typeof(TestDatabase) }
            .SelectMany(type => type.Assembly.ManifestModule.ModuleVersionId.ToByteArray())
            .ToArray();

        return TemplatePrefix + Convert.ToHexStringLower(SHA256.HashData(versions))[..16];
    }

    private static async Task EnsureTemplateAsync(NpgsqlConnection connection, string serverConnectionString, string templateName)
    {
        var databases = await GetTemplateNamesAsync(connection);

        if (databases.Contains(templateName))
        {
            return;
        }

        // Building under another name means a run that dies part way through can't leave a broken template behind.
        await ExecuteAsync(connection, $"drop database if exists {TemplateBuildName} with (force)");
        await ExecuteAsync(connection, $"create database {TemplateBuildName}");

        var buildConnectionString = new NpgsqlConnectionStringBuilder(WithDatabase(serverConnectionString, TemplateBuildName))
        {
            // A template can't be cloned while anything is connected to it.
            Pooling = false
        }.ConnectionString;

        await using (var dbContext = TrsDbContext.Create(buildConnectionString))
        {
            await dbContext.Database.MigrateAsync();
            await DbHelper.SeedDbAsync(dbContext);
        }

        await ExecuteAsync(connection, $"alter database {TemplateBuildName} rename to {templateName}");

        foreach (var oldTemplate in (await GetTemplateNamesAsync(connection)).Skip(TemplatesToKeep))
        {
            await ExecuteAsync(connection, $"drop database {oldTemplate} with (force)");
        }
    }

    private static async Task<List<string>> GetTemplateNamesAsync(NpgsqlConnection connection)
    {
        await using var command = new NpgsqlCommand(
            $"select datname from pg_database where starts_with(datname, '{TemplatePrefix}') order by oid desc",
            connection);
        await using var reader = await command.ExecuteReaderAsync();

        var names = new List<string>();

        while (await reader.ReadAsync())
        {
            names.Add(reader.GetString(0));
        }

        return names;
    }

    private static async Task<string> CreateDatabaseInFreeSlotAsync(NpgsqlConnection connection, string templateName)
    {
        for (var slot = TemplateLockKey + 1; ; slot++)
        {
            await using var command = new NpgsqlCommand($"select pg_try_advisory_lock({AdvisoryLockClass}, {slot})", connection);

            if ((bool)(await command.ExecuteScalarAsync())!)
            {
                var databaseName = $"trs_test_{slot}";
                await ExecuteAsync(connection, $"drop database if exists {databaseName} with (force)");
                await ExecuteAsync(connection, $"create database {databaseName} template {templateName} strategy file_copy");
                return databaseName;
            }
        }
    }

    private static string WithDatabase(string serverConnectionString, string databaseName) =>
        new NpgsqlConnectionStringBuilder(serverConnectionString) { Database = databaseName }.ConnectionString;

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql)
    {
        // Waiting on the template lock can take as long as another process's template build.
        await using var command = new NpgsqlCommand(sql, connection) { CommandTimeout = 0 };
        await command.ExecuteNonQueryAsync();
    }

    private sealed record Lease(string ConnectionString, NpgsqlConnection LockConnection);
}

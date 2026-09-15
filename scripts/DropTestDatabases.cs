#!/usr/bin/env -S dotnet --

using Microsoft.Extensions.Configuration;
using Npgsql;

// Every test project creates a database named for the repository it ran from and the project, so they build up as
// worktrees and branches come and go. They're all disposable — the next run recreates whatever it needs.
const string TestDatabaseNamePattern = "^trs_[0-9a-f]{8}_";

var configuration = new ConfigurationManager();
configuration
    .AddUserSecrets("TeachingRecordSystemTests")
    .AddEnvironmentVariables();

var connectionString = configuration.GetConnectionString("DefaultConnection");

if (bool.TryParse(configuration["UseTestContainers"], out var useTestContainers) && useTestContainers || connectionString is null)
{
    var port = int.TryParse(configuration["TestContainersPostgresPort"], out var configuredPort) ? configuredPort : 43007;
    connectionString = $"Host=localhost;Port={port};Database=trs;Username=postgres;Password=postgres;";
}

var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString) { Database = "postgres" };

await using var connection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
await connection.OpenAsync();

Console.WriteLine($"Looking for test databases on {connectionStringBuilder.Host}:{connectionStringBuilder.Port}.");

var databaseNames = new List<string>();

await using (var command = connection.CreateCommand())
{
    command.CommandText = "select datname from pg_database where datname ~ @pattern order by datname";
    command.Parameters.AddWithValue("pattern", TestDatabaseNamePattern);

    await using var reader = await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        databaseNames.Add(reader.GetString(0));
    }
}

if (databaseNames.Count == 0)
{
    Console.WriteLine("No test databases to drop.");
    return 0;
}

var dropped = 0;

foreach (var databaseName in databaseNames)
{
    // Asking first means something else running tests gets a clear message rather than a broken connection
    if (await GetConnectionCountAsync(databaseName) > 0)
    {
        Console.WriteLine($"Skipped {databaseName}, it's in use.");
        continue;
    }

    await using var command = connection.CreateCommand();
    command.CommandText = $"drop database \"{databaseName}\"";

    try
    {
        await command.ExecuteNonQueryAsync();
        dropped++;
        Console.WriteLine($"Dropped {databaseName}.");
    }
    catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ObjectInUse)
    {
        // Something else is running tests against it; it isn't ours to take away mid-run
        Console.WriteLine($"Skipped {databaseName}, it's in use.");
    }
    catch (Exception ex)
    {
        // One database we can't drop shouldn't stop us clearing the rest
        Console.WriteLine($"Skipped {databaseName}: {ex.Message}");
    }
}

Console.WriteLine($"Dropped {dropped} of {databaseNames.Count} test databases.");

return 0;

async Task<long> GetConnectionCountAsync(string databaseName)
{
    await using var command = connection.CreateCommand();
    command.CommandText = "select count(*) from pg_stat_activity where datname = @databaseName";
    command.Parameters.AddWithValue("databaseName", databaseName);

    return (long)(await command.ExecuteScalarAsync())!;
}

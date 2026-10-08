#!/usr/bin/env -S dotnet --

#:package Microsoft.Extensions.Configuration@10.0.12
#:package Microsoft.Extensions.Configuration.Binder@10.0.12
#:package Microsoft.Extensions.Configuration.EnvironmentVariables@10.0.12
#:package Microsoft.Extensions.Configuration.UserSecrets@10.0.12
#:package Npgsql@10.0.3

using Microsoft.Extensions.Configuration;
using Npgsql;

// The tests build a template database per schema and clone a pool of databases from it, all named trs_tmpl_* and
// trs_test_*. They're disposable - the next run recreates whatever it needs - but old schemas, and databases kept
// back for a failing test, are never cleaned up by the tests themselves.
const string TestDatabaseNamePattern = "^trs_(tmpl|test)_";
const int DefaultTestContainersPostgresPort = 43007;

var configuration = new ConfigurationManager();
configuration
    .AddUserSecrets("TeachingRecordSystemTests")
    .AddEnvironmentVariables();

// Mirrors TestConfiguration: UseTestContainers, or no connection string at all, means the testcontainer.
var connectionString = configuration.GetConnectionString("DefaultConnection");

if (configuration.GetValue<bool>("UseTestContainers") || connectionString is null)
{
    var port = configuration.GetValue<int?>("TestContainersPostgresPort") ?? DefaultTestContainersPostgresPort;
    connectionString = $"Host=localhost;Port={port};Database=trs;Username=postgres;Password=postgres;";
}

var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString) { Database = "postgres", Pooling = false };

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
    // A test run holds an advisory lock named for each database it has claimed and, shared with other runs, for the
    // template it clones them from, for as long as it lasts; a template being built is locked under its scratch name.
    // Connections alone would miss a database that's claimed but idle, or a template between clones.
    if (!await TryLockAsync(databaseName) || await GetConnectionCountAsync(databaseName) > 0)
    {
        Console.WriteLine($"Skipped {databaseName}, it's in use.");
        continue;
    }

    try
    {
        // A template can't be dropped until it's no longer marked as one.
        await ExecuteAsync($"alter database \"{databaseName}\" with is_template false");
        await ExecuteAsync($"drop database \"{databaseName}\"");
        dropped++;
        Console.WriteLine($"Dropped {databaseName}.");
    }
    catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ObjectInUse)
    {
        // Something started cloning it, or connected to it, since we checked
        Console.WriteLine($"Skipped {databaseName}, it's in use.");
    }
}

Console.WriteLine($"Dropped {dropped} of {databaseNames.Count} test databases.");

return 0;

async Task<bool> TryLockAsync(string databaseName)
{
    await using var command = connection.CreateCommand();
    command.CommandText = "select pg_try_advisory_lock(hashtextextended(@name, 0))";
    command.Parameters.AddWithValue("name", databaseName);

    return (bool)(await command.ExecuteScalarAsync())!;
}

async Task<long> GetConnectionCountAsync(string databaseName)
{
    await using var command = connection.CreateCommand();
    command.CommandText = "select count(*) from pg_stat_activity where datname = @name";
    command.Parameters.AddWithValue("name", databaseName);

    return (long)(await command.ExecuteScalarAsync())!;
}

async Task ExecuteAsync(string sql)
{
    await using var command = connection.CreateCommand();
    command.CommandText = sql;
    await command.ExecuteNonQueryAsync();
}

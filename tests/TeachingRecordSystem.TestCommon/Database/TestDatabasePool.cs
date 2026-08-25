using System.Threading.Channels;
using Npgsql;

namespace TeachingRecordSystem.TestCommon.Database;

public sealed class PooledTestDatabase(string name, NpgsqlDataSource dataSource, string connectionString, string resetStatement)
{
    public string Name { get; } = name;

    // Kept separately because NpgsqlDataSource.ConnectionString redacts the password.
    public string ConnectionString { get; } = connectionString;

    public NpgsqlDataSource DataSource { get; } = dataSource;

    public async Task ResetAsync()
    {
        await using var connection = await DataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = resetStatement;
        await command.ExecuteNonQueryAsync();

        PooledReferenceDataCaches.Invalidate(Name);
    }
}

// A fixed set of databases cloned from the template, leased exclusively to one test at a time.
//
// Databases are created on demand rather than up front, so running a single test creates a single database.
// That is what keeps the edit/run/fix loop as cheap as it is with no isolation at all.
public sealed class TestDatabasePool(TestDatabaseServer server, TestDatabaseTemplate template, int maxSize) : IAsyncDisposable
{
    private readonly Channel<PooledTestDatabase> _available = Channel.CreateUnbounded<PooledTestDatabase>();
    private readonly List<PooledTestDatabase> _all = [];
    private readonly Lock _allGate = new();
    private int _slots;
    private int _nameCounter;

    public async ValueTask<TestDatabaseLease> AcquireAsync(CancellationToken cancellationToken)
    {
        if (_available.Reader.TryRead(out var ready))
        {
            return new TestDatabaseLease(ready, this);
        }

        if (Interlocked.Increment(ref _slots) <= maxSize)
        {
            return new TestDatabaseLease(await CreateDatabaseAsync(), this);
        }

        Interlocked.Decrement(ref _slots);
        return new TestDatabaseLease(await _available.Reader.ReadAsync(cancellationToken), this);
    }

    // For hosts that serve real HTTP requests, where there is no per-test context to hang the lease on.
    public async ValueTask<TestDatabaseLease> AcquireForRunAsync()
    {
        var database = _available.Reader.TryRead(out var ready) ? ready : await CreateDatabaseAsync();
        Interlocked.Increment(ref _slots);
        return new TestDatabaseLease(database, this, registerAsCurrent: false);
    }

    internal async ValueTask ReturnAsync(PooledTestDatabase database)
    {
        await database.ResetAsync();
        await _available.Writer.WriteAsync(database);
    }

    // A retained database must not go back into circulation or the next test inherits its rows. Freeing the
    // slot lets the pool build a replacement on demand.
    internal ValueTask RetireAsync(PooledTestDatabase database)
    {
        Interlocked.Decrement(ref _slots);

        lock (_allGate)
        {
            _all.Remove(database);
        }

        return database.DataSource.DisposeAsync();
    }

    private async Task<PooledTestDatabase> CreateDatabaseAsync()
    {
        // The state key is part of the name, so a database left behind by a previous run is only picked up
        // while both its schema and the reset semantics still match. Truncating one of those is much cheaper
        // than cloning the template again.
        var name = $"trs_test_{template.StateKey}_{Interlocked.Increment(ref _nameCounter):D3}";

        var exists = await server.ExecuteScalarAsync<int?>(
            "select 1 from pg_database where datname = @name",
            ("name", name)) is not null;

        if (!exists)
        {
            await server.ExecuteAsync($"create database \"{name}\" template \"{template.Name}\"");
        }

        var connectionString = server.ConnectionStringFor(name);
        var dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();
        var database = new PooledTestDatabase(name, dataSource, connectionString, template.ResetStatement);

        if (exists)
        {
            await database.ResetAsync();
        }

        lock (_allGate)
        {
            _all.Add(database);
        }

        return database;
    }

    public async ValueTask DisposeAsync()
    {
        PooledTestDatabase[] all;
        lock (_allGate)
        {
            all = _all.ToArray();
        }

        foreach (var database in all)
        {
            await database.DataSource.DisposeAsync();
        }
    }
}

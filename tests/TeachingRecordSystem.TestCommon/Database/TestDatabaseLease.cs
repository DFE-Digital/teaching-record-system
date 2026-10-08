using Npgsql;

namespace TeachingRecordSystem.TestCommon.Database;

public sealed class TestDatabaseLease : IAsyncDisposable
{
    private readonly PooledTestDatabase _database;
    private readonly TestDatabasePool _pool;
    private readonly bool _registered = true;
    private bool _retain;

    internal TestDatabaseLease(PooledTestDatabase database, TestDatabasePool pool, bool registerAsCurrent = true)
    {
        _database = database;
        _pool = pool;

        if (registerAsCurrent)
        {
            TestDatabaseScope.Register(this);
        }
        else
        {
            _registered = false;
        }
    }

    public NpgsqlDataSource DataSource => _database.DataSource;

    public string DatabaseName => _database.Name;

    public string ConnectionString => _database.ConnectionString;

    // Keeps the database out of circulation with its data intact so a failure can be inspected with psql.
    public void Retain() => _retain = true;

    public ValueTask DisposeAsync()
    {
        if (_registered)
        {
            TestDatabaseScope.Unregister(this);
        }

        return _retain ? _pool.RetireAsync(_database) : _pool.ReturnAsync(_database);
    }
}

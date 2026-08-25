using Microsoft.Extensions.Configuration;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.TestCommon.Database;

// Entry point for the pooled-database infrastructure. Call InitializeAsync once per test run (from an
// ITestPipelineStartup or an assembly fixture), then AcquireAsync once per test.
public static class TestDatabases
{
    private static readonly SemaphoreSlim _initGate = new(1, 1);
    private static readonly SortedDictionary<string, Func<TrsDbContext, Task>> _templateSeeds = new(StringComparer.Ordinal);
    private static TestDatabasePool? _pool;

    public static TestDatabasePool Pool => _pool
        ?? throw new InvalidOperationException(
            $"{nameof(TestDatabases)}.{nameof(InitializeAsync)} has not been called for this test run.");

    public static bool IsInitialized => _pool is not null;

    // Registers data that every test database should start with. Anything a test project used to write once
    // at host startup belongs here instead: with a database per test, a startup task would only ever populate
    // whichever database happened to be leased at the time.
    //
    // The key identifies the seed and is part of the template's name, so changing a seed means bumping its
    // key rather than hunting for stale templates.
    public static void AddTemplateSeed(string key, Func<TrsDbContext, Task> seed)
    {
        if (IsInitialized)
        {
            throw new InvalidOperationException("Template seeds must be registered before the pool is initialized.");
        }

        _templateSeeds[key] = seed;
    }

    public static async Task InitializeAsync()
    {
        await _initGate.WaitAsync();

        try
        {
            if (_pool is not null)
            {
                return;
            }

            var server = await TestDatabaseServer.EnsureStartedAsync();
            var template = await TestDatabaseTemplate.EnsureAsync(server, _templateSeeds);
            _pool = new TestDatabasePool(server, template, GetMaxPoolSize());
        }
        finally
        {
            _initGate.Release();
        }
    }

    public static ValueTask<TestDatabaseLease> AcquireAsync(CancellationToken cancellationToken) =>
        Pool.AcquireAsync(cancellationToken);

    public static ValueTask<TestDatabaseLease> AcquireForRunAsync() => Pool.AcquireForRunAsync();

    public static async Task DisposeAsync()
    {
        if (_pool is not null)
        {
            await _pool.DisposeAsync();
            _pool = null;
        }
    }

    // Headroom over the thread count so a test that is waiting on something else isn't holding the only
    // spare database.
    private static int GetMaxPoolSize() =>
        TestConfiguration.GetConfiguration().GetValue<int?>("TestDatabasePoolSize") is int configured && configured > 0
            ? configured
            : Environment.ProcessorCount + 4;
}

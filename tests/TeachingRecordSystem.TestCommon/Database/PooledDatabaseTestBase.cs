using Xunit;

namespace TeachingRecordSystem.TestCommon.Database;

// Leases one database for the duration of a single test. Because the lease is exclusive there is nothing to
// share and nothing to clean up afterwards, so tests derived from this need neither [ClearDbBeforeTest] nor a
// DisableParallelization collection.
public abstract class PooledDatabaseTestBase : IAsyncLifetime
{
    private TestDatabaseLease? _lease;

    protected string DatabaseName =>
        _lease?.DatabaseName ?? throw new InvalidOperationException("No database has been leased.");

    // For code that builds its own host from configuration rather than resolving from the test's container -
    // the CLI commands, for instance - and so can't pick up the ambient data source.
    protected string DatabaseConnectionString =>
        _lease?.ConnectionString ?? throw new InvalidOperationException("No database has been leased.");

    public virtual async ValueTask InitializeAsync() =>
        _lease = await TestDatabases.AcquireAsync(TestContext.Current.CancellationToken);

    public virtual async ValueTask DisposeAsync()
    {
        if (_lease is null)
        {
            return;
        }

        // Keep a failing test's data so it can be inspected: psql -d <name>
        if (TestContext.Current.TestState?.Result == TestResult.Failed)
        {
            TestContext.Current.SendDiagnosticMessage($"Retained test database '{_lease.DatabaseName}'.");
            _lease.Retain();
        }

        await _lease.DisposeAsync();
        _lease = null;
    }
}

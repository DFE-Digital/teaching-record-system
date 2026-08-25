using System.Collections.Concurrent;
using Npgsql;
using Xunit;

namespace TeachingRecordSystem.TestCommon.Database;

// Tracks which pooled database the currently-running test owns.
//
// This deliberately does not use an AsyncLocal the way TestScopedServices does. TestScopedServices gets away
// with it because Reset() is called from a test class's *constructor*, which shares an execution context with
// the test method. Acquiring a database is async, so it has to happen in InitializeAsync, and a value assigned
// to an AsyncLocal inside an async method is not visible to its caller. Keying off TestContext instead works
// from anywhere xUnit has flowed its own context, which includes the TestServer request pipeline when
// PreserveExecutionContext is set.
public static class TestDatabaseScope
{
    private static readonly ConcurrentDictionary<string, TestDatabaseLease> _leases = new();

    public static TestDatabaseLease Current => TryGetCurrent()
        ?? throw new InvalidOperationException(
            "No test database has been leased. Does the test's base class acquire one in InitializeAsync?");

    public static NpgsqlDataSource CurrentDataSource => Current.DataSource;

    public static TestDatabaseLease? TryGetCurrent() =>
        CurrentKey is string key && _leases.TryGetValue(key, out var lease) ? lease : null;

    internal static void Register(TestDatabaseLease lease)
    {
        var key = CurrentKey ?? throw new InvalidOperationException(
            "A test database can only be leased from inside a test.");

        if (!_leases.TryAdd(key, lease))
        {
            throw new InvalidOperationException($"A test database is already leased for '{key}'.");
        }
    }

    internal static void Unregister(TestDatabaseLease lease)
    {
        if (CurrentKey is string key)
        {
            _leases.TryRemove(key, out _);
        }
    }

    private static string? CurrentKey => TestContext.Current.Test?.UniqueID;
}

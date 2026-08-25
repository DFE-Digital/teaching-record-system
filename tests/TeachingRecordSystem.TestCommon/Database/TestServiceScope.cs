using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace TeachingRecordSystem.TestCommon.Database;

// A DI scope per test.
//
// Tests routinely resolve services straight off the host's root provider. Resolving a *scoped* service that
// way caches it in the root scope for the lifetime of the run, so the first test to resolve, say, a service
// holding a TrsDbContext pins every later test to that first test's database. With one shared database this
// was invisible; with a database per test it silently reads and writes the wrong one.
//
// Fixtures expose Current in place of the root provider so those resolutions land in a scope that lives and
// dies with the test.
public static class TestServiceScope
{
    private static readonly ConcurrentDictionary<string, IServiceScope> _scopes = new();

    public static IServiceProvider? Current =>
        CurrentKey is string key && _scopes.TryGetValue(key, out var scope) ? scope.ServiceProvider : null;

    public static IDisposable Push(IServiceProvider rootServices)
    {
        var key = CurrentKey ?? throw new InvalidOperationException("A service scope can only be pushed from a test.");
        var scope = rootServices.CreateScope();

        if (!_scopes.TryAdd(key, scope))
        {
            scope.Dispose();
            throw new InvalidOperationException($"A service scope is already active for '{key}'.");
        }

        return new Popper(key);
    }

    private static string? CurrentKey => TestContext.Current.Test?.UniqueID;

    private sealed class Popper(string key) : IDisposable
    {
        public void Dispose()
        {
            if (_scopes.TryRemove(key, out var scope))
            {
                scope.Dispose();
            }
        }
    }
}

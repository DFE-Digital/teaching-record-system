using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace TeachingRecordSystem.TestCommon.Database;

// The application caches rows it has read - the enabled webhook endpoints, person info - in a process-wide
// IMemoryCache. Once each test has its own database that cache would hand one test's rows to another: a webhook
// message created against another database's endpoint fails on a foreign key, for instance.
//
// This is the same problem PooledReferenceDataCaches solves for ReferenceDataCache, solved the same way: one cache
// per pooled database, emptied when the database is reset. Anything running outside a test's scope - a fixture, or
// a host serving real HTTP with a run-scoped lease - gets a cache of its own.
public static class PooledMemoryCaches
{
    private const string NoTestDatabase = "";

    private static readonly ConcurrentDictionary<string, MemoryCache> _caches = new();

    public static IMemoryCache ForCurrentDatabase() =>
        _caches.GetOrAdd(
            TestDatabaseScope.TryGetCurrent()?.DatabaseName ?? NoTestDatabase,
            _ => new MemoryCache(Options.Create(new MemoryCacheOptions())));

    internal static void Invalidate(string databaseName)
    {
        if (_caches.TryGetValue(databaseName, out var cache))
        {
            cache.Clear();
        }
    }
}

// Registered as the application's IMemoryCache; each call is routed to the cache for the running test's database.
internal sealed class PooledMemoryCache : IMemoryCache
{
    public ICacheEntry CreateEntry(object key) => PooledMemoryCaches.ForCurrentDatabase().CreateEntry(key);

    public void Remove(object key) => PooledMemoryCaches.ForCurrentDatabase().Remove(key);

    public bool TryGetValue(object key, out object? value) => PooledMemoryCaches.ForCurrentDatabase().TryGetValue(key, out value);

    // The caches outlive any one container, so disposing a container mustn't dispose them.
    public void Dispose()
    {
    }
}

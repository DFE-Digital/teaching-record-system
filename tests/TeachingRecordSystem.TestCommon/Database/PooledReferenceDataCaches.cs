using System.Collections.Concurrent;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.TestCommon.Database;

// ReferenceDataCache is a process-wide singleton in the application, which stops being safe once each test
// has its own database: a test that adds reference data (RefreshTrainingProvidersJob, for example) would
// otherwise publish the new rows' ids to every other test through the shared cache, and a concurrent test
// referencing one from a different database fails on a foreign key.
//
// One cache per pooled database keeps each cache consistent with the database it describes.
public static class PooledReferenceDataCaches
{
    private static readonly ConcurrentDictionary<string, ReferenceDataCache> _caches = new();

    public static ReferenceDataCache ForCurrentDatabase(IDbContextFactory<TrsDbContext> dbContextFactory) =>
        _caches.GetOrAdd(TestDatabaseScope.Current.DatabaseName, _ => new ReferenceDataCache(dbContextFactory));

    // Called when a database is reset. The reference tables themselves survive a reset, so this only forces
    // the next read to come from the database rather than from a cache built by a previous test.
    internal static void Invalidate(string databaseName)
    {
        if (_caches.TryGetValue(databaseName, out var cache))
        {
            cache.Clear();
        }
    }
}

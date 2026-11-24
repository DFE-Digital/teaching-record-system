using System.Transactions;

namespace TeachingRecordSystem.Core.DataStore.Postgres;

public static class UniqueValueLockExtensions
{
    /// <summary>
    /// Takes a lock on <paramref name="value"/> for the unique index <paramref name="indexName"/>,
    /// held until the current transaction ends.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Call this before checking whether a value is already in use and then writing it. Concurrent requests for
    /// the same value wait for the lock, so the second request's check sees the first request's row instead of
    /// both passing the check and the second write failing on the unique index.
    /// </para>
    /// <para>
    /// Prefer this over catching a unique index violation: a failed statement aborts the whole transaction,
    /// which then can't be committed.
    /// </para>
    /// </remarks>
    public static async Task AcquireUniqueValueLockAsync(
        this TrsDbContext dbContext,
        string indexName,
        string value,
        CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.CurrentTransaction is null && Transaction.Current is null)
        {
            throw new InvalidOperationException("A transaction is required; the lock is released when it ends.");
        }

        var key = $"{indexName}:{value}";

        // Different keys can hash to the same lock; that only means unrelated requests occasionally wait for each other
        await dbContext.Database.ExecuteSqlAsync(
            $"select pg_advisory_xact_lock(hashtext({key}))",
            cancellationToken);
    }
}

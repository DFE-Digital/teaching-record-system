using System.Collections.Concurrent;
using System.Reactive.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Services.CrmEntityChanges;

namespace TeachingRecordSystem.TestCommon;

public sealed class TestableCrmEntityChangesService : ICrmEntityChangesService, IDisposable
{
    private readonly ConcurrentDictionary<string, System.Reactive.Subjects.ReplaySubject<IChangedItem[]>> _entityTypeSubjects = new();
    private bool _disposed = false;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        var subjects = _entityTypeSubjects.Values.ToArray();

        foreach (var s in subjects)
        {
            s.Dispose();
        }

        _entityTypeSubjects.Clear();
        _disposed = true;
    }

    public IObserver<IChangedItem[]> GetChangedItemsObserver(string entityLogicalName)
    {
        ThrowIfDisposed();

        return _entityTypeSubjects.GetOrAdd(entityLogicalName, _ => new System.Reactive.Subjects.ReplaySubject<IChangedItem[]>());
    }

    public IAsyncEnumerable<IChangedItem[]> GetEntityChanges(
        string changesKey,
        string entityLogicalName,
        ColumnSet columns,
        DateTime? modifiedSince,
        int pageSize = 1000,
        bool rollUpChanges = true,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var subject = _entityTypeSubjects.GetOrAdd(entityLogicalName, _ => new System.Reactive.Subjects.ReplaySubject<IChangedItem[]>());

        return subject
            .Select(batch => batch
                .Where(e => !modifiedSince.HasValue || e is not NewOrUpdatedItem ||
                e is NewOrUpdatedItem newOrUpdatedItem && newOrUpdatedItem.NewOrUpdatedEntity.GetAttributeValue<DateTime>("modifiedon") >= modifiedSince.Value)
                .ToArray()
            )
            .Where(batch => batch.Length > 0)
            .Select(batch =>
                batch.GroupBy(e =>
                    !rollUpChanges ? Guid.NewGuid() :
                    e is NewOrUpdatedItem newOrUpdatedItem ? newOrUpdatedItem.NewOrUpdatedEntity.Id :
                    e is RemovedOrDeletedItem removedOrDeletedItem ? removedOrDeletedItem.RemovedItem.Id :
                    throw new NotSupportedException($"Unexpected ChangeType: '{e.Type}'."))
                .Select(g => g.Last())
                .ToArray())
            .ToAsyncEnumerable();
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);
}

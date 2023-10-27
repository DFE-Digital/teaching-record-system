using System.Collections.Concurrent;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Services.CrmEntityChanges;

namespace TeachingRecordSystem.Core.Dqt.Tests.Services;

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
        string serviceClientName,
        string entityLogicalName,
        ColumnSet columns,
        int pageSize = 1000,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var subject = _entityTypeSubjects.GetOrAdd(entityLogicalName, _ => new System.Reactive.Subjects.ReplaySubject<IChangedItem[]>());
        return subject.ToAsyncEnumerable();
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);
}

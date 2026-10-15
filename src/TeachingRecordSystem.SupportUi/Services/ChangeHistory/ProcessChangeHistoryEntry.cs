using System.Diagnostics.CodeAnalysis;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Services.ChangeHistory;

public record ProcessChangeHistoryEntry(Process Process, RaisedByUserInfo RaisedByUser, ChangeHistoryContext Context)
{
    public bool ContainsEvent<T>() where T : IEvent => Process.Events!.Select(pe => pe.Payload).OfType<T>().Any();

    public T GetEvent<T>() where T : IEvent => Process.Events!.Select(pe => pe.Payload).OfType<T>().Single();

    public IReadOnlyCollection<T> GetEvents<T>(Guid personId) where T : IEvent =>
        Process.Events!.Select(pe => pe.Payload).OfType<T>().Where(e => e.PersonIds.Contains(personId)).AsReadOnly();

    public bool TryGetEvent<T>([NotNullWhen(true)] out T? @event) where T : IEvent
    {
        @event = Process.Events!.Select(pe => pe.Payload).OfType<T>().SingleOrDefault();
        return @event is not null;
    }
}

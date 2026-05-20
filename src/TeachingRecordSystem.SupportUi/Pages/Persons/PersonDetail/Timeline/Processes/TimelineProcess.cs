using System.Diagnostics.CodeAnalysis;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.Timeline.Processes;

public record TimelineProcess(Process Process, RaisedByUserInfo RaisedByUser, IReadOnlyDictionary<Guid, PersonInfo> PersonInfo)
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

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class ProcessEvent
{
    public required Guid ProcessEventId { get; init; }
    public required Guid ProcessId { get; init; }
    public required string EventName { get; init; }
    public required IEvent Payload { get; init; }
    public required ICollection<Guid> PersonIds { get; init; }
    public required DateTime CreatedOn { get; init; }
}

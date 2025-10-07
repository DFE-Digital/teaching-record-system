namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class ActionEvent
{
    public required Guid ActionEventId { get; init; }
    public required Guid ActionId { get; init; }
    public required string EventName { get; init; }
    public required IEvent Payload { get; init; }
    public required ICollection<Guid> PersonIds { get; init; }
}

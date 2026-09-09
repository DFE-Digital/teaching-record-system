namespace TeachingRecordSystem.Core.Events;

public record PersonUnmergedEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => [PersonId, UnmergedFromPersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required Guid PersonId { get; init; }
    public required Guid UnmergedFromPersonId { get; init; }
}

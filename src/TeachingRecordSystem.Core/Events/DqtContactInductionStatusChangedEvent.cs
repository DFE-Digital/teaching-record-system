namespace TeachingRecordSystem.Core.Events;

public record DqtContactInductionStatusChangedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [PersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required Guid PersonId { get; init; }
    public required string? InductionStatus { get; init; }
    public required string? OldInductionStatus { get; init; }
}

namespace TeachingRecordSystem.Core.Events;

public record OneLoginUserSignedInEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => [];
    public required string Subject { get; init; }
}

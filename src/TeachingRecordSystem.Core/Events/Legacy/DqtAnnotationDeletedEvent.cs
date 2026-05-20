namespace TeachingRecordSystem.Core.Events.Legacy;

public record DqtAnnotationDeletedEvent : EventBase
{
    public required Guid AnnotationId { get; init; }
}

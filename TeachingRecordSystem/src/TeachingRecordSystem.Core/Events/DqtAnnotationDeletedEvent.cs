namespace TeachingRecordSystem.Core.Events;

public record DqtAnnotationDeletedEvent : EventBase
{
    public required Guid AnnotationId { get; init; }
}

namespace TeachingRecordSystem.Core.Events;

public record EmailSentEvent : EventBase
{
    public required EventModels.Email Email { get; init; }
}

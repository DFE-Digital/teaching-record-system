namespace TeachingRecordSystem.Core.Events.Legacy;

public record EmailSentEvent : EventBase
{
    public required EventModels.Email Email { get; init; }
}

namespace TeachingRecordSystem.Core.Events;

public abstract record SupportTaskUpdatedEvent : EventBase
{
    public required EventModels.SupportTask SupportTask { get; init; }
    public required EventModels.SupportTask OldSupportTask { get; init; }
}

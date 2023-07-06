namespace TeachingRecordSystem.Core.Events;

public abstract record EventBase
{
    public required DateTime CreatedUtc { get; init; }
}

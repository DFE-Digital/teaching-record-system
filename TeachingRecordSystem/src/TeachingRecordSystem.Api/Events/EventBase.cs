namespace TeachingRecordSystem.Api.Events;

public abstract record EventBase
{
    public required DateTime CreatedUtc { get; init; }
}

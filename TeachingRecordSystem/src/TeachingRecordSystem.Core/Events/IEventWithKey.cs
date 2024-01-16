namespace TeachingRecordSystem.Core.Events;

public interface IEventWithKey
{
    string? Key { get; }
}

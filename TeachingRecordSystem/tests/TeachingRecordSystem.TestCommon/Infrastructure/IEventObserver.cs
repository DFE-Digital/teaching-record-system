namespace TeachingRecordSystem.TestCommon.Infrastructure;

public interface IEventObserver
{
    void OnEventCreated(EventBase @event);
}

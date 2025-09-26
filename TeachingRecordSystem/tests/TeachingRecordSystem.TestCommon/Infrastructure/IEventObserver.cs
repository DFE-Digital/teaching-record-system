namespace TeachingRecordSystem.TestCommon.Infrastructure;

public interface IEventObserver
{
#pragma warning disable CA1716
    void OnEventCreated(EventBase @event);
#pragma warning restore CA1716
}

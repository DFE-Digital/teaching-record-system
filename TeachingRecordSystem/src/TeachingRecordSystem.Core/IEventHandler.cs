namespace TeachingRecordSystem.Core;

#pragma warning disable CA1711
public interface IEventHandler
#pragma warning restore CA1711
{
    Task HandleEventAsync(IEvent @event, ProcessContext processContext);
}

public interface IEventHandler<TEvent> where TEvent : IEvent
{
    Task HandleEventAsync(TEvent @event, ProcessContext processContext);
}

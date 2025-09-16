namespace TeachingRecordSystem.Core.Events.EventHandlers;

public interface IEventHandler
{
    Task HandleAsync(EventBase @event);
}

public interface IEventHandler<in TEvent> where TEvent : EventBase
{
    Task HandleAsync(TEvent @event);
}

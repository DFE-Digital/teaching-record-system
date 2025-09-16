using TeachingRecordSystem.Core.Events.EventHandlers;

namespace TeachingRecordSystem.TestCommon.Infrastructure;

public class EventObserverEventHandler(IEventObserver eventObserver) : IEventHandler
{
    public Task HandleAsync(EventBase @event)
    {
        eventObserver.OnEventCreated(@event);
        return Task.CompletedTask;
    }
}

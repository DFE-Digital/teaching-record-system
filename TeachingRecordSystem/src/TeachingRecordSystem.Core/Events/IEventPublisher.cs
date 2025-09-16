namespace TeachingRecordSystem.Core.Events;

public interface IEventPublisher
{
    Task PublishEventAsync(EventBase @event);
}

public static class EventPublisherExtensions
{
    public static async Task PublishEventsAsync(this IEventPublisher eventPublisher, IEnumerable<EventBase> events)
    {
        foreach (var @event in events)
        {
            await eventPublisher.PublishEventAsync(@event);
        }
    }
}

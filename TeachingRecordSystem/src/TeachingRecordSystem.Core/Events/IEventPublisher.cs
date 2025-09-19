namespace TeachingRecordSystem.Core.Events;

public interface IEventPublisher
{
    Task PublishEventAsync(EventBase @event);
}

public static class EventPublisherExtensions
{
    public static Task PublishEventsAsync(this IEventPublisher eventPublisher, IEnumerable<EventBase> events) =>
        Task.WhenAll(events.Select(eventPublisher.PublishEventAsync));
}

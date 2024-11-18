namespace TeachingRecordSystem.Core.Events.Processing;

public interface IEventPublisher
{
    Task PublishEventAsync(EventBase @event);
}

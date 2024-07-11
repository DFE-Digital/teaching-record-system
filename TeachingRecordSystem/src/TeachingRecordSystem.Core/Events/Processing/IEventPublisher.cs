namespace TeachingRecordSystem.Core.Events.Processing;

public interface IEventPublisher
{
    Task PublishEvent(EventBase @event);
}

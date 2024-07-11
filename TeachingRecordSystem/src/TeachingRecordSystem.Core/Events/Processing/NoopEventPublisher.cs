namespace TeachingRecordSystem.Core.Events.Processing;

public class NoopEventPublisher : IEventPublisher
{
    public Task PublishEvent(EventBase @event) => Task.CompletedTask;
}

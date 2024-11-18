namespace TeachingRecordSystem.Core.Events.Processing;

public class NoopEventPublisher : IEventPublisher
{
    public Task PublishEventAsync(EventBase @event) => Task.CompletedTask;
}

namespace TeachingRecordSystem.TestCommon;

public class TestEventPublisher : IEventPublisher
{
    public Task PublishEventAsync(IEvent @event, ProcessContext processContext)
    {
        return Task.CompletedTask;
    }
}

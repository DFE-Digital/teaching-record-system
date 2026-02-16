namespace TeachingRecordSystem.TestCommon;

public class TestEventPublisher : IEventPublisher
{
    public IEventScope GetOrCreateEventScope(ProcessContext processContext)
    {
        return new TestEventScope();
    }

    private class TestEventScope : IEventScope
    {
        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public Task PublishEventAsync(IEvent @event)
        {
            return Task.CompletedTask;
        }
    }
}

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

        public Task PublishEventAsync(IEvent @event, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}

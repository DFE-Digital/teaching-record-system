using Xunit;

namespace TeachingRecordSystem.TestCommon;

public class EventCapture : IEventHandler
{
    private readonly List<EventAndProcess> _events = [];

    public void Clear() => _events.Clear();

    public void AssertEventsPublished(params Action<IEvent>[] eventInspectors) =>
        Assert.Collection(_events.Select(t => t.Event), eventInspectors);

    public void AssertProcessesAndEventsPublished(params Action<EventAndProcess>[] eventAndProcessInspectors) =>
        Assert.Collection(_events, eventAndProcessInspectors);

    public void AssertNoEventsPublished() =>
        Assert.Empty(_events);

    public Task HandleEventAsync(IEvent @event, ProcessContext processContext)
    {
        _events.Add(new EventAndProcess(@event, processContext));
        return Task.CompletedTask;
    }

    public record EventAndProcess(IEvent Event, ProcessContext ProcessContext);
}

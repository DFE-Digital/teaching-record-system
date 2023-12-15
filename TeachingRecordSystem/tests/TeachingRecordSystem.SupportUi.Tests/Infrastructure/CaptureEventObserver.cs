using TeachingRecordSystem.Core.Events;
using TeachingRecordSystem.Core.Events.Processing;

namespace TeachingRecordSystem.SupportUi.Tests.Infrastructure;

public class CaptureEventObserver : IEventObserver
{
    private readonly List<EventBase> _events = new();

    public void Clear() => _events.Clear();

    public Task OnEventSaved(EventBase @event)
    {
        _events.Add(@event);
        return Task.CompletedTask;
    }

    public void AssertEventsSaved(params Action<EventBase>[] eventInspectors)
    {
        var events = _events.AsReadOnly();
        Assert.Collection(events, eventInspectors);
    }
}

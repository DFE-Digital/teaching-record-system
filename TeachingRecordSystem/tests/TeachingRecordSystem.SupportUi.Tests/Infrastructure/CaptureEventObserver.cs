using TeachingRecordSystem.Core.Events;
using TeachingRecordSystem.Core.Events.Processing;

namespace TeachingRecordSystem.SupportUi.Tests.Infrastructure;

public class CaptureEventObserver : IEventObserver
{
    private readonly AsyncLocal<List<EventBase>> _events = new();

    public void Clear() => _events.Value?.Clear();

    public void Init() => _events.Value ??= new List<EventBase>();

    public Task OnEventSaved(EventBase @event)
    {
        if (_events.Value is null)
        {
            throw new InvalidOperationException("Not initialized.");
        }

        _events.Value.Add(@event);

        return Task.CompletedTask;
    }

    public void AssertEventsSaved(params Action<EventBase>[] eventInspectors)
    {
        var events = (_events.Value ?? new()).AsReadOnly();
        Assert.Collection(events, eventInspectors);
    }
}

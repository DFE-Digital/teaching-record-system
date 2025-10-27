using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.TestCommon.Infrastructure;
using Xunit;

namespace TeachingRecordSystem.TestCommon;

public class CaptureEventObserver : IEventObserver
{
    private readonly List<EventBase> _events = [];

    public void Clear() => _events.Clear();

    public void OnEventCreated(EventBase @event) => _events.Add(@event);

    public void AssertEventsSaved(params Action<EventBase>[] eventInspectors) =>
        Assert.Collection(_events, eventInspectors);

    public void AssertNoEventsSaved() =>
        Assert.Empty(_events);
}

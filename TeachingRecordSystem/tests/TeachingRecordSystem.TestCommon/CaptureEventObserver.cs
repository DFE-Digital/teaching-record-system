using System.Diagnostics.CodeAnalysis;
using TeachingRecordSystem.Core.Events.Processing;
using Xunit;

namespace TeachingRecordSystem.TestCommon;

public class CaptureEventObserver : IEventObserver
{
    private readonly HashSet<EventBase> _events = new(new EventIdEqualityComparer());

    public void Clear() => _events.Clear();

    public Task OnEventSaved(EventBase @event)
    {
        _events.Add(@event);
        return Task.CompletedTask;
    }

    public void AssertEventsSaved(params Action<EventBase>[] eventInspectors) =>
        Assert.Collection(_events, eventInspectors);

    private class EventIdEqualityComparer : IEqualityComparer<EventBase>
    {
        public bool Equals(EventBase? x, EventBase? y) => x?.EventId == y?.EventId;

        public int GetHashCode([DisallowNull] EventBase obj) => obj.EventId.GetHashCode();
    }
}

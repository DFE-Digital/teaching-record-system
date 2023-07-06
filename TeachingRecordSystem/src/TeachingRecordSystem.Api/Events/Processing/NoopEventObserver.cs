using TeachingRecordSystem.Core.Events;

namespace TeachingRecordSystem.Api.Events.Processing;

public class NoopEventObserver : IEventObserver
{
    public Task OnEventSaved(EventBase @event) => Task.CompletedTask;
}

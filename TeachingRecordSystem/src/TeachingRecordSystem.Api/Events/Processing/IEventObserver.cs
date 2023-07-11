using TeachingRecordSystem.Core.Events;

namespace TeachingRecordSystem.Api.Events.Processing;

public interface IEventObserver
{
    Task OnEventSaved(EventBase @event);
}

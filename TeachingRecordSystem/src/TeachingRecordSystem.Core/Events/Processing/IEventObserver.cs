namespace TeachingRecordSystem.Core.Events.Processing;

public interface IEventObserver
{
    Task OnEventSaved(EventBase @event);
}

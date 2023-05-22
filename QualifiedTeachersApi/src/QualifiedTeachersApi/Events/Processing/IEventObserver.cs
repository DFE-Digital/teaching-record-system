namespace QualifiedTeachersApi.Events.Processing;

public interface IEventObserver
{
    Task OnEventSaved(EventBase @event);
}

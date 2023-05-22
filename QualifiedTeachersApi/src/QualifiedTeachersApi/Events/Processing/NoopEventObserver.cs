namespace QualifiedTeachersApi.Events.Processing;

public class NoopEventObserver : IEventObserver
{
    public Task OnEventSaved(EventBase @event) => Task.CompletedTask;
}

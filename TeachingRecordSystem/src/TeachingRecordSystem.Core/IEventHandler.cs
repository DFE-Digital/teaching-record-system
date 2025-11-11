namespace TeachingRecordSystem.Core;

public interface IEventHandler
{
    Task HandleEventAsync(IEvent @event, ProcessContext processContext);
}

namespace TeachingRecordSystem.Core;

#pragma warning disable CA1711
public interface IEventHandler
#pragma warning restore CA1711
{
    Task HandleEventAsync(IEvent @event, ProcessContext processContext);
}

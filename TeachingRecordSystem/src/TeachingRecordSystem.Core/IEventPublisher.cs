using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core;

public interface IEventPublisher
{
    Task PublishEventAsync(IEvent @event, ProcessContext processContext);
}

public class EventPublisher(TrsDbContext dbContext) : IEventPublisher
{
    public async Task PublishEventAsync(IEvent @event, ProcessContext processContext)
    {
        if (@event.RaisedBy.UserId != processContext.Process.UserId)
        {
            throw new InvalidOperationException("The user for the event does not match the user for the process.");
        }

        if (dbContext.Entry(processContext.Process).State == EntityState.Detached)
        {
            dbContext.Set<Process>().Add(processContext.Process);
        }

        @event.PersonIds.ForEach(e => processContext.Process.PersonIds.Add(e));

        var processEvent = new ProcessEvent
        {
            ProcessEventId = @event.EventId,
            ProcessId = processContext.Process.ProcessId,
            EventName = @event.GetType().Name,
            Payload = @event,
            PersonIds = @event.PersonIds
        };
        dbContext.Set<ProcessEvent>().Add(processEvent);

        await dbContext.SaveChangesAsync();
    }
}

public class ProcessContext
{
    public ProcessContext(ProcessType processType, DateTime now, Guid userId)
    {
        Process = new Process
        {
            ProcessId = Guid.NewGuid(),
            ProcessType = processType,
            Created = now,
            UserId = userId,
            PersonIds = []
        };
    }

    public Process Process { get; }

    public ProcessType ProcessType => Process.ProcessType;
}

using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core;

public interface IEventPublisher
{
    Task PublishEventAsync(IEvent @event, ProcessContext processContext);
}

public class EventPublisher(TrsDbContext dbContext, IServiceProvider serviceProvider) : IEventPublisher
{
    public async Task PublishEventAsync(IEvent @event, ProcessContext processContext)
    {
        if (dbContext.Entry(processContext.Process).State == EntityState.Detached)
        {
            dbContext.Set<Process>().Add(processContext.Process);
        }

        processContext.Process.UpdatedOn = processContext.Now;

        @event.PersonIds.Except(processContext.Process.PersonIds).ForEach(e => processContext.Process.PersonIds.Add(e));

        var processEvent = new ProcessEvent
        {
            ProcessEventId = @event.EventId,
            ProcessId = processContext.Process.ProcessId,
            EventName = @event.GetType().Name,
            Payload = @event,
            PersonIds = @event.PersonIds,
            CreatedOn = processContext.Now
        };
        dbContext.Set<ProcessEvent>().Add(processEvent);

        await dbContext.SaveChangesAsync();

        processContext.AddEvent(@event);

        await InvokeEventHandlersAsync(@event, processContext);
    }

    private async Task InvokeEventHandlersAsync(IEvent @event, ProcessContext processContext)
    {
        var handlers = serviceProvider.GetServices<IEventHandler>();

        foreach (var handler in handlers)
        {
            await handler.HandleEventAsync(@event, processContext);
        }

        var eventType = @event.GetType();
        var typeSpecificHandlers = serviceProvider.GetServices(typeof(IEventHandler<>).MakeGenericType(eventType));

        foreach (var handler in typeSpecificHandlers)
        {
            var wrapper = (IEventHandler)Activator.CreateInstance(typeof(TypedHandlerWrapper<>).MakeGenericType(eventType), handler)!;
            await wrapper.HandleEventAsync(@event, processContext);
        }
    }

    private class TypedHandlerWrapper<TEvent>(IEventHandler<TEvent> innerHandler) : IEventHandler where TEvent : IEvent
    {
        public Task HandleEventAsync(IEvent @event, ProcessContext processContext)
        {
            return innerHandler.HandleEventAsync(((TEvent)@event), processContext);
        }
    }
}

public class ProcessContext
{
    private readonly List<IEvent> _events = new();

    public ProcessContext(ProcessType processType, DateTime now, EventModels.RaisedByUserInfo raisedBy)
    {
        Now = now;

        Process = new Process
        {
            ProcessId = Guid.NewGuid(),
            ProcessType = processType,
            CreatedOn = now,
            UpdatedOn = now,
            UserId = raisedBy.UserId,
            DqtUserId = raisedBy.DqtUserId,
            DqtUserName = raisedBy.DqtUserName,
            PersonIds = [],
            Events = []
        };
    }

    private ProcessContext(Process process, DateTime now)
    {
        Now = now;
        Process = process;
        _events = process.Events?.Select(e => e.Payload).ToList() ?? throw new InvalidOperationException("Process must have its Events loaded.");
    }

    public static async Task<ProcessContext> FromDbAsync(TrsDbContext dbContext, Guid processId, DateTime now)
    {
        var process = await dbContext.Processes
            .Include(p => p.Events)
            .SingleAsync(p => p.ProcessId == processId);

        return new(process, now);
    }

    public DateTime Now { get; }

    public IReadOnlyCollection<Guid> PersonIds => Process.PersonIds;

    public IReadOnlyCollection<IEvent> Events => _events.AsReadOnly();

    public Process Process { get; }

    public Guid ProcessId => Process.ProcessId;

    public ProcessType ProcessType => Process.ProcessType;

    public Guid UserId => Process.UserId!.Value;

    [EditorBrowsable(EditorBrowsableState.Never)]  // This is meant to be consumed by EventPublisher only
    internal void AddEvent(IEvent @event)
    {
        _events.Add(@event);
    }
}

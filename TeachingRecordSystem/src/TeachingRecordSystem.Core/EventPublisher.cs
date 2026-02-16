using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core;

public interface IEventPublisher
{
    IEventScope GetOrCreateEventScope(ProcessContext processContext);

    public async Task PublishSingleEventAsync(IEvent @event, ProcessContext processContext)
    {
        await using var scope = GetOrCreateEventScope(processContext);
        await scope.PublishEventAsync(@event);
    }
}

public interface IEventScope : IAsyncDisposable
{
    Task PublishEventAsync(IEvent @event);
}

public class EventPublisher(TrsDbContext dbContext, IServiceProvider serviceProvider) : IEventPublisher
{
    private static readonly AsyncLocal<RootScope?> _rootScope = new();

    public IEventScope GetOrCreateEventScope(ProcessContext processContext)
    {
        if (_rootScope.Value is { } rootScope)
        {
            if (!ReferenceEquals(rootScope.ProcessContext, processContext))
            {
                throw new InvalidOperationException("Existing event scope is associated with a different ProcessContext.");
            }

            return new ChildScope(rootScope);
        }

        rootScope = new RootScope(processContext, dbContext, serviceProvider);
        _rootScope.Value = rootScope;
        return rootScope;
    }

    private sealed class ChildScope(RootScope rootScope) : IEventScope
    {
        ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;

        public Task PublishEventAsync(IEvent @event) => rootScope.PublishEventAsync(@event);
    }

    private sealed class RootScope(
        ProcessContext processContext,
        TrsDbContext dbContext,
        IServiceProvider serviceProvider) : IEventScope
    {
        private readonly List<IEvent> _events = new();
        private bool _published;

        public ProcessContext ProcessContext => processContext;

        public async Task PublishEventAsync(IEvent @event)
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
                OneLoginUserSubjects = @event.OneLoginUserSubjects,
                CreatedOn = processContext.Now
            };
            dbContext.Set<ProcessEvent>().Add(processEvent);

            await dbContext.SaveChangesAsync();

            processContext.AddEvent(@event);
            _events.Add(@event);
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (_published)
            {
                return;
            }

            await ProcessEventsAsync();

            async Task ProcessEventsAsync()
            {
                var events = _events.ToArray();
                _events.Clear();

                foreach (var @event in events)
                {
                    await InvokeEventHandlersAsync(@event);
                }

                if (_events.Count > 0)
                {
                    await ProcessEventsAsync();
                }
            }

            _published = true;
        }

        private async Task InvokeEventHandlersAsync(IEvent @event)
        {
            var handlers = serviceProvider.GetServices<IEventHandler>();

            foreach (var handler in handlers)
            {
                await handler.HandleEventAsync(@event, processContext, this);
            }

            var eventType = @event.GetType();
            var typeSpecificHandlers = serviceProvider.GetServices(typeof(IEventHandler<>).MakeGenericType(eventType));

            foreach (var handler in typeSpecificHandlers)
            {
                var wrapper = (IEventHandler)Activator.CreateInstance(typeof(TypedHandlerWrapper<>).MakeGenericType(eventType), handler)!;
                await wrapper.HandleEventAsync(@event, processContext, this);
            }
        }

        private class TypedHandlerWrapper<TEvent>(IEventHandler<TEvent> innerHandler) : IEventHandler where TEvent : IEvent
        {
            public Task HandleEventAsync(IEvent @event, ProcessContext processContext, IEventScope eventScope)
            {
                return innerHandler.HandleEventAsync(((TEvent)@event), processContext, eventScope);
            }
        }
    }
}

public class ProcessContext
{
    private readonly List<IEvent> _events = new();

    public ProcessContext(ProcessType processType, DateTime now, EventModels.RaisedByUserInfo raisedBy, IChangeReason? changeReason = null)
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
            Events = [],
            ChangeReason = changeReason
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

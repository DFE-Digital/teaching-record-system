using Xunit;

namespace TeachingRecordSystem.TestCommon;

public class EventCapture : IEventHandler
{
    private readonly Dictionary<Guid, ProcessAndEvents> _processes = [];

    public void Clear() => _processes.Clear();

    public void AssertEventsPublished(params Action<IEvent>[] eventInspectors) =>
        Assert.Collection(_processes.Values.SelectMany(t => t.Events), eventInspectors);

    public void AssertProcessesCreated(params Action<ProcessAndEvents>[] eventAndProcessInspectors) =>
        Assert.Collection(_processes.Values, eventAndProcessInspectors);

    public void AssertNoEventsPublished() =>
        Assert.Empty(_processes);

    public Task HandleEventAsync(IEvent @event, ProcessContext processContext)
    {
        if (!_processes.TryGetValue(processContext.ProcessId, out var processAndEvents))
        {
            processAndEvents = new ProcessAndEvents(processContext);
            _processes.Add(processContext.ProcessId, processAndEvents);
        }

        processAndEvents.AddEvent(@event);

        return Task.CompletedTask;
    }

    public record ProcessAndEvents
    {
        private readonly List<IEvent> _events;

        public ProcessAndEvents(ProcessContext processContext)
        {
            ProcessContext = processContext;
            _events = new();
        }

        public ProcessContext ProcessContext { get; }

        public IReadOnlyCollection<IEvent> Events => _events.AsReadOnly();

        internal void AddEvent(IEvent @event) => _events.Add(@event);
    }
}

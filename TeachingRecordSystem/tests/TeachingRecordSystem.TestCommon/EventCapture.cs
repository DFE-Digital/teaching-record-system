using Xunit;
using Xunit.Sdk;

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

        public void AssertProcessHasEvent<TEvent>(Action<TEvent>? inspector = null) where TEvent : IEvent
        {
            var events = _events.OfType<TEvent>().ToArray();

            if (events.Length == 0)
            {
                throw new XunitException($"No {typeof(TEvent).Name} events found on process.");
            }

            if (events.Length > 1)
            {
                throw new XunitException($"Multiple {typeof(TEvent).Name} events found on process.");
            }

            inspector?.Invoke(events[0]);
        }

        public void AssertProcessHasEvents<T1>(
            Action<T1>? inspector1 = null)
            where T1 : IEvent
        {
            Assert.Equal(1, _events.Count);
            AssertProcessHasEvent(inspector1);
        }

        public void AssertProcessHasEvents<T1, T2>(
            Action<T1>? inspector1 = null,
            Action<T2>? inspector2 = null)
            where T1 : IEvent
            where T2 : IEvent
        {
            Assert.Equal(2, _events.Count);
            AssertProcessHasEvent(inspector1);
            AssertProcessHasEvent(inspector2);
        }

        public void AssertProcessHasEvents<T1, T2, T3>(
            Action<T1>? inspector1 = null,
            Action<T2>? inspector2 = null,
            Action<T3>? inspector3 = null)
            where T1 : IEvent
            where T2 : IEvent
            where T3 : IEvent
        {
            Assert.Equal(3, _events.Count);
            AssertProcessHasEvent(inspector1);
            AssertProcessHasEvent(inspector2);
            AssertProcessHasEvent(inspector3);
        }

        public void AssertProcessHasEvents<T1, T2, T3, T4>(
            Action<T1>? inspector1 = null,
            Action<T2>? inspector2 = null,
            Action<T3>? inspector3 = null,
            Action<T4>? inspector4 = null)
            where T1 : IEvent
            where T2 : IEvent
            where T3 : IEvent
            where T4 : IEvent
        {
            Assert.Equal(4, _events.Count);
            AssertProcessHasEvent(inspector1);
            AssertProcessHasEvent(inspector2);
            AssertProcessHasEvent(inspector3);
            AssertProcessHasEvent(inspector4);
        }
    }
}

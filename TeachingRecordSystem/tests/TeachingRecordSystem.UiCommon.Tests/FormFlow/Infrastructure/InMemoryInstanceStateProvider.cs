using TeachingRecordSystem.UiCommon.FormFlow;
using TeachingRecordSystem.UiCommon.FormFlow.State;

namespace TeachingRecordSystem.UiCommon.Tests.FormFlow.Infrastructure;

public class InMemoryInstanceStateProvider : IUserInstanceStateProvider
{
    private readonly Dictionary<string, Entry> _instances;

    public InMemoryInstanceStateProvider()
    {
        _instances = new();
    }

    public void Clear() => _instances.Clear();

    public Task<JourneyInstance> CreateInstanceAsync(
        JourneyInstanceId instanceId,
        Type stateType,
        object state,
        IReadOnlyDictionary<object, object>? properties)
    {
        _instances.Add(instanceId, new Entry()
        {
            StateType = stateType,
            State = state,
            Properties = properties
        });

        var instance = JourneyInstance.Create(
            this,
            instanceId,
            stateType,
            state,
            properties ?? PropertiesBuilder.CreateEmpty());

        return Task.FromResult(instance);
    }

    public Task CompleteInstanceAsync(JourneyInstanceId instanceId, Type stateType)
    {
        _instances[instanceId].Completed = true;
        return Task.CompletedTask;
    }

    public Task DeleteInstanceAsync(JourneyInstanceId instanceId, Type stateType)
    {
        _instances.Remove(instanceId);
        return Task.CompletedTask;
    }

    public Task<JourneyInstance?> GetInstanceAsync(JourneyInstanceId instanceId, Type stateType)
    {
        _instances.TryGetValue(instanceId, out var entry);

        var instance = entry != null ?
            JourneyInstance.Create(this, instanceId, entry.StateType!, entry.State!, entry.Properties!, entry.Completed) :
            null;

        return Task.FromResult(instance);
    }

    public Task UpdateInstanceStateAsync(JourneyInstanceId instanceId, Type stateType, object state)
    {
        _instances[instanceId].State = state;
        return Task.CompletedTask;
    }

    private class Entry
    {
        public IReadOnlyDictionary<object, object>? Properties { get; set; }
        public object? State { get; set; }
        public Type? StateType { get; set; }
        public bool Completed { get; set; }
    }
}

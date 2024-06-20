using System.Collections.Concurrent;
using System.Text.Json;
using TeachingRecordSystem.UiCommon.FormFlow;
using TeachingRecordSystem.UiCommon.FormFlow.State;

namespace TeachingRecordSystem.UiTestCommon.Infrastructure.FormFlow;

public class InMemoryInstanceStateProvider : IUserInstanceStateProvider
{
    private readonly ConcurrentDictionary<string, Entry> _instances;

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
        _instances.TryAdd(instanceId, new Entry()
        {
            StateType = stateType,
            State = CloneState(state, stateType),
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
        _instances.Remove(instanceId, out _);
        return Task.CompletedTask;
    }

    public Task<JourneyInstance?> GetInstanceAsync(JourneyInstanceId instanceId, Type stateType)
    {
        _instances.TryGetValue(instanceId, out var entry);

        var instance = entry != null ?
            JourneyInstance.Create(this, instanceId, entry.StateType!, CloneState(entry.State!, entry.StateType!), entry.Properties!, entry.Completed) :
            null;

        return Task.FromResult(instance);
    }

    public Task UpdateInstanceStateAsync(JourneyInstanceId instanceId, Type stateType, object state)
    {
        _instances[instanceId].State = CloneState(state, stateType);
        return Task.CompletedTask;
    }

    private static object CloneState(object state, Type stateType) =>
        JsonSerializer.Deserialize(
            JsonSerializer.Serialize(state, stateType)!,
            stateType)!;

    private class Entry
    {
        public IReadOnlyDictionary<object, object>? Properties { get; set; }
        public object? State { get; set; }
        public Type? StateType { get; set; }
        public bool Completed { get; set; }
    }
}

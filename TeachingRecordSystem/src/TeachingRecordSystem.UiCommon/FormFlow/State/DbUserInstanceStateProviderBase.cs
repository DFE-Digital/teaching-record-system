using System.Text.Json;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.UiCommon.FormFlow.State;

public abstract class DbUserInstanceStateProviderBase(IClock clock, IOptions<JsonOptions> jsonOptionsAccessor) : IUserInstanceStateProvider
{
    protected IClock Clock { get; } = clock;

    public abstract Task CompleteInstanceAsync(JourneyInstanceId instanceId, Type stateType);

    public abstract Task<JourneyInstance> CreateInstanceAsync(
        JourneyInstanceId instanceId,
        Type stateType,
        object state,
        IReadOnlyDictionary<object, object>? properties);

    public abstract Task DeleteInstanceAsync(JourneyInstanceId instanceId, Type stateType);

    public abstract Task<JourneyInstance?> GetInstanceAsync(JourneyInstanceId instanceId, Type stateType);

    public abstract Task UpdateInstanceStateAsync(JourneyInstanceId instanceId, Type stateType, object state);

    protected async Task CompleteInstanceAsync(JourneyInstanceId instanceId, Type stateType, string userId, TrsDbContext dbContext)
    {
        var instance = await dbContext.JourneyStates.SingleOrDefaultAsync(j => j.InstanceId == instanceId && j.UserId == userId) ??
            throw new ArgumentException("Instance does not exist.");

        if (instance.Completed is not null)
        {
            throw new InvalidOperationException("Instance is already completed.");
        }

        instance.Completed = Clock.UtcNow;
        await dbContext.SaveChangesAsync();
    }

    protected async Task<JourneyInstance> CreateInstanceAsync(
        JourneyInstanceId instanceId,
        Type stateType,
        object state,
        IReadOnlyDictionary<object, object>? properties,
        string userId,
        TrsDbContext dbContext)
    {
        if (properties is { Count: > 0 })
        {
            throw new NotSupportedException("Specifying properties is not supported.");
        }

        var serializedState = SerializeState(stateType, state);
        var instance = new JourneyState()
        {
            InstanceId = instanceId,
            UserId = userId,
            State = serializedState,
            Created = Clock.UtcNow,
            Updated = Clock.UtcNow
        };
        dbContext.JourneyStates.Add(instance);
        await dbContext.SaveChangesAsync();

        return JourneyInstance.Create(this, instanceId, stateType, state, PropertiesBuilder.CreateEmpty(), completed: false);
    }

    protected async Task DeleteInstanceAsync(JourneyInstanceId instanceId, Type stateType, string userId, TrsDbContext dbContext)
    {
        var instance = await dbContext.JourneyStates.SingleOrDefaultAsync(j => j.InstanceId == instanceId && j.UserId == userId) ??
            throw new ArgumentException("Instance does not exist.");

        dbContext.JourneyStates.Remove(instance);
        await dbContext.SaveChangesAsync();
    }

    protected async Task<JourneyInstance?> GetInstanceAsync(JourneyInstanceId instanceId, Type stateType, string userId, TrsDbContext dbContext)
    {
        var instance = await dbContext.JourneyStates.SingleOrDefaultAsync(j => j.InstanceId == instanceId && j.UserId == userId);

        if (instance is null)
        {
            return null;
        }

        var state = DeserializeState(stateType, instance.State);

        return JourneyInstance.Create(this, instanceId, stateType, state, PropertiesBuilder.CreateEmpty(), completed: instance.Completed.HasValue);
    }

    protected async Task UpdateInstanceStateAsync(JourneyInstanceId instanceId, Type stateType, object state, string userId, TrsDbContext dbContext)
    {
        var instance = await dbContext.JourneyStates.SingleOrDefaultAsync(j => j.InstanceId == instanceId && j.UserId == userId) ??
            throw new ArgumentException("Instance does not exist.");

        var serializedState = SerializeState(stateType, state);
        instance.State = serializedState;
        instance.Updated = Clock.UtcNow;
        await dbContext.SaveChangesAsync();
    }

    private string SerializeState(Type stateType, object state) =>
        JsonSerializer.Serialize(state, stateType, jsonOptionsAccessor.Value.JsonSerializerOptions);

    private object DeserializeState(Type stateType, string serialized) =>
        JsonSerializer.Deserialize(serialized, stateType, jsonOptionsAccessor.Value.JsonSerializerOptions)!;
}

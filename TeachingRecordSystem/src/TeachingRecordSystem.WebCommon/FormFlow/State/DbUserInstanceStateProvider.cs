using System.Text.Json;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.FormFlow;

namespace TeachingRecordSystem.WebCommon.FormFlow.State;

public class DbUserInstanceStateProvider(
    TrsDbContext dbContext,
    ICurrentUserIdProvider currentUserIdProvider,
    IClock clock,
    IOptions<JsonOptions> jsonOptionsAccessor) :
    IUserInstanceStateProvider
{
    public async Task CompleteInstanceAsync(JourneyInstanceId instanceId, Type stateType)
    {
        var userId = currentUserIdProvider.GetCurrentUserId();

        var instance = await GetInstanceAsync(instanceId, userId) ??
            throw new ArgumentException("Instance does not exist.");

        if (instance.Completed is not null)
        {
            throw new InvalidOperationException("Instance is already completed.");
        }

        instance.Completed = clock.UtcNow;
        await dbContext.SaveChangesAsync();
    }

    public async Task<JourneyInstance> CreateInstanceAsync(JourneyInstanceId instanceId, Type stateType, object state)
    {
        var userId = currentUserIdProvider.GetCurrentUserId();

        var serializedState = SerializeState(stateType, state);
        var instance = new JourneyState()
        {
            InstanceId = instanceId,
            UserId = userId,
            State = serializedState,
            Created = clock.UtcNow,
            Updated = clock.UtcNow
        };
        dbContext.JourneyStates.Add(instance);
        await dbContext.SaveChangesAsync();

        return JourneyInstance.Create(this, instanceId, stateType, state, completed: false);
    }

    public async Task DeleteInstanceAsync(JourneyInstanceId instanceId, Type stateType)
    {
        var userId = currentUserIdProvider.GetCurrentUserId();

        var instance = await GetInstanceAsync(instanceId, userId) ??
            throw new ArgumentException("Instance does not exist.");

        dbContext.JourneyStates.Remove(instance);
        await dbContext.SaveChangesAsync();
    }

    public async Task<JourneyInstance?> GetInstanceAsync(JourneyInstanceId instanceId, Type stateType)
    {
        var userId = currentUserIdProvider.GetCurrentUserId();

        var instance = await GetInstanceAsync(instanceId, userId);

        if (instance is null)
        {
            return null;
        }

        var state = DeserializeState(stateType, instance.State);

        return JourneyInstance.Create(this, instanceId, stateType, state, completed: instance.Completed.HasValue);
    }

    public async Task UpdateInstanceStateAsync(JourneyInstanceId instanceId, Type stateType, object state)
    {
        var userId = currentUserIdProvider.GetCurrentUserId();

        var instance = await GetInstanceAsync(instanceId, userId) ??
            throw new ArgumentException("Instance does not exist.");

        var serializedState = SerializeState(stateType, state);
        instance.State = serializedState;
        instance.Updated = clock.UtcNow;
        await dbContext.SaveChangesAsync();
    }

    private async Task<JourneyState?> GetInstanceAsync(JourneyInstanceId instanceId, string userId)
    {
        return await dbContext.JourneyStates
            .FromSql(
                $"""
                 SELECT *
                 FROM journey_states
                 WHERE instance_id = {instanceId.ToString()} AND user_id = {userId}
                 FOR UPDATE
                 """)
            .SingleOrDefaultAsync();
    }

    private string SerializeState(Type stateType, object state) =>
        JsonSerializer.Serialize(state, stateType, jsonOptionsAccessor.Value.JsonSerializerOptions);

    private object DeserializeState(Type stateType, string serialized) =>
        JsonSerializer.Deserialize(serialized, stateType, jsonOptionsAccessor.Value.JsonSerializerOptions)!;
}

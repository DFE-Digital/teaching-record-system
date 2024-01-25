using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.FormFlow;

namespace TeachingRecordSystem.FormFlow.State;

public class DbUserInstanceStateProvider(
    IDbContextFactory<TrsDbContext> dbContextFactory,
    ICurrentUserIdProvider currentUserIdProvider,
    IClock clock,
    IOptions<JsonOptions> jsonOptionsAccessor) : IUserInstanceStateProvider
{
    public async Task CompleteInstanceAsync(JourneyInstanceId instanceId, Type stateType)
    {
        using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var userId = currentUserIdProvider.GetCurrentUserId();

        var instance = await dbContext.JourneyStates.SingleOrDefaultAsync(j => j.InstanceId == instanceId && j.UserId == userId) ??
            throw new ArgumentException("Instance does not exist.");

        if (instance.Completed is not null)
        {
            throw new InvalidOperationException("Instance is already completed.");
        }

        instance.Completed = clock.UtcNow;
        await dbContext.SaveChangesAsync();
    }

    public async Task<JourneyInstance> CreateInstanceAsync(
        JourneyInstanceId instanceId,
        Type stateType,
        object state,
        IReadOnlyDictionary<object, object>? properties)
    {
        if (properties is { Count: > 0 })
        {
            throw new NotSupportedException("Specifying properties is not supported.");
        }

        using var dbContext = await dbContextFactory.CreateDbContextAsync();

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

        return JourneyInstance.Create(this, instanceId, stateType, state, PropertiesBuilder.CreateEmpty(), completed: false);
    }

    public async Task DeleteInstanceAsync(JourneyInstanceId instanceId, Type stateType)
    {
        using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var userId = currentUserIdProvider.GetCurrentUserId();

        var instance = await dbContext.JourneyStates.SingleOrDefaultAsync(j => j.InstanceId == instanceId && j.UserId == userId) ??
            throw new ArgumentException("Instance does not exist.");

        dbContext.JourneyStates.Remove(instance);
        await dbContext.SaveChangesAsync();
    }

    public async Task<JourneyInstance?> GetInstanceAsync(JourneyInstanceId instanceId, Type stateType)
    {
        using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var userId = currentUserIdProvider.GetCurrentUserId();

        var instance = await dbContext.JourneyStates.SingleOrDefaultAsync(j => j.InstanceId == instanceId && j.UserId == userId);

        if (instance is null)
        {
            return null;
        }

        var state = DeserializeState(stateType, instance.State);

        return JourneyInstance.Create(this, instanceId, stateType, state, PropertiesBuilder.CreateEmpty(), completed: instance.Completed.HasValue);
    }

    public async Task UpdateInstanceStateAsync(JourneyInstanceId instanceId, Type stateType, object state)
    {
        using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var userId = currentUserIdProvider.GetCurrentUserId();

        var instance = await dbContext.JourneyStates.SingleOrDefaultAsync(j => j.InstanceId == instanceId && j.UserId == userId) ??
            throw new ArgumentException("Instance does not exist.");

        var serializedState = SerializeState(stateType, state);
        instance.State = serializedState;
        instance.Updated = clock.UtcNow;
        await dbContext.SaveChangesAsync();
    }

    private string SerializeState(Type stateType, object state) =>
        JsonSerializer.Serialize(state, stateType, jsonOptionsAccessor.Value.JsonSerializerOptions);

    private object DeserializeState(Type stateType, string serialized) =>
        JsonSerializer.Deserialize(serialized, stateType, jsonOptionsAccessor.Value.JsonSerializerOptions)!;
}

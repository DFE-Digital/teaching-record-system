using System.Text.Json;
using FormFlow.State;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Infrastructure.FormFlow;

public class DbUserInstanceStateProvider : IUserInstanceStateProvider
{
    private readonly TrsDbContext _dbContext;
    private readonly ICurrentUserIdProvider _currentUserIdProvider;
    private readonly IClock _clock;
    private readonly IOptions<JsonOptions> _jsonOptionsAccessor;

    public DbUserInstanceStateProvider(
        TrsDbContext dbContext,
        ICurrentUserIdProvider currentUserIdProvider,
        IClock clock,
        IOptions<JsonOptions> jsonOptionsAccessor)
    {
        _dbContext = dbContext;
        _currentUserIdProvider = currentUserIdProvider;
        _clock = clock;
        _jsonOptionsAccessor = jsonOptionsAccessor;
    }

    public async Task CompleteInstanceAsync(JourneyInstanceId instanceId, Type stateType)
    {
        var userId = _currentUserIdProvider.GetCurrentUserId();

        var instance = await _dbContext.JourneyStates.SingleOrDefaultAsync(j => j.InstanceId == instanceId && j.UserId == userId) ??
            throw new ArgumentException("Instance does not exist.");

        if (instance.Completed is not null)
        {
            throw new InvalidOperationException("Instance is already completed.");
        }

        instance.Completed = _clock.UtcNow;
        await _dbContext.SaveChangesAsync();
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

        var userId = _currentUserIdProvider.GetCurrentUserId();

        var serializedState = SerializeState(stateType, state);
        var instance = new JourneyState()
        {
            InstanceId = instanceId,
            UserId = userId,
            State = serializedState,
            Created = _clock.UtcNow,
            Updated = _clock.UtcNow
        };
        _dbContext.JourneyStates.Add(instance);
        await _dbContext.SaveChangesAsync();

        return JourneyInstance.Create(this, instanceId, stateType, state, PropertiesBuilder.CreateEmpty(), completed: false);
    }

    public async Task DeleteInstanceAsync(JourneyInstanceId instanceId, Type stateType)
    {
        var userId = _currentUserIdProvider.GetCurrentUserId();

        var instance = await _dbContext.JourneyStates.SingleOrDefaultAsync(j => j.InstanceId == instanceId && j.UserId == userId) ??
            throw new ArgumentException("Instance does not exist.");

        _dbContext.JourneyStates.Remove(instance);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<JourneyInstance?> GetInstanceAsync(JourneyInstanceId instanceId, Type stateType)
    {
        var userId = _currentUserIdProvider.GetCurrentUserId();

        var instance = await _dbContext.JourneyStates.SingleOrDefaultAsync(j => j.InstanceId == instanceId && j.UserId == userId);

        if (instance is null)
        {
            return null;
        }

        var state = DeserializeState(stateType, instance.State);

        return JourneyInstance.Create(this, instanceId, stateType, state, PropertiesBuilder.CreateEmpty(), completed: instance.Completed.HasValue);
    }

    public async Task UpdateInstanceStateAsync(JourneyInstanceId instanceId, Type stateType, object state)
    {
        var userId = _currentUserIdProvider.GetCurrentUserId();

        var instance = await _dbContext.JourneyStates.SingleOrDefaultAsync(j => j.InstanceId == instanceId && j.UserId == userId) ??
            throw new ArgumentException("Instance does not exist.");

        var serializedState = SerializeState(stateType, state);
        instance.State = serializedState;
        instance.Updated = _clock.UtcNow;
        await _dbContext.SaveChangesAsync();
    }

    private string SerializeState(Type stateType, object state) =>
        JsonSerializer.Serialize(state, stateType, _jsonOptionsAccessor.Value.JsonSerializerOptions);

    private object DeserializeState(Type stateType, string serialized) =>
        JsonSerializer.Deserialize(serialized, stateType, _jsonOptionsAccessor.Value.JsonSerializerOptions)!;
}

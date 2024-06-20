namespace TeachingRecordSystem.UiCommon.FormFlow.State;

public interface IUserInstanceStateProvider
{
    Task CompleteInstanceAsync(JourneyInstanceId instanceId, Type stateType);

    Task<JourneyInstance> CreateInstanceAsync(
        JourneyInstanceId instanceId,
        Type stateType,
        object state,
        IReadOnlyDictionary<object, object>? properties);

    Task DeleteInstanceAsync(JourneyInstanceId instanceId, Type stateType);

    Task<JourneyInstance?> GetInstanceAsync(JourneyInstanceId instanceId, Type stateType);

    Task UpdateInstanceStateAsync(JourneyInstanceId instanceId, Type stateType, object state);
}

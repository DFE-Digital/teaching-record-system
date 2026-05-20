namespace TeachingRecordSystem.WebCommon.FormFlow.State;

public interface IUserInstanceStateProvider
{
    Task CompleteInstanceAsync(JourneyInstanceId instanceId, Type stateType);

    Task<JourneyInstance> CreateInstanceAsync(JourneyInstanceId instanceId, Type stateType, object state);

    Task DeleteInstanceAsync(JourneyInstanceId instanceId, Type stateType);

    Task<JourneyInstance?> GetInstanceAsync(JourneyInstanceId instanceId, Type stateType);

    Task UpdateInstanceStateAsync(JourneyInstanceId instanceId, Type stateType, object state);
}

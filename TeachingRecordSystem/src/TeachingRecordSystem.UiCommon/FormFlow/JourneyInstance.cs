using TeachingRecordSystem.UiCommon.FormFlow.State;

namespace TeachingRecordSystem.UiCommon.FormFlow;

public class JourneyInstance
{
    private readonly IUserInstanceStateProvider _stateProvider;

    internal JourneyInstance(
        IUserInstanceStateProvider stateProvider,
        JourneyInstanceId instanceId,
        Type stateType,
        object state,
        IReadOnlyDictionary<object, object> properties,
        bool completed = false)
    {
        _stateProvider = stateProvider;
        StateType = stateType;
        InstanceId = instanceId;
        Properties = properties ?? PropertiesBuilder.CreateEmpty();
        State = state;
        Completed = completed;
    }

    public bool Completed { get; internal set; }

    public bool Deleted { get; internal set; }

    public string JourneyName => InstanceId.JourneyName;

    public JourneyInstanceId InstanceId { get; }

    public IReadOnlyDictionary<object, object> Properties { get; }

    public object State { get; private set; }

    public Type StateType { get; }

    public static JourneyInstance Create(
        IUserInstanceStateProvider stateProvider,
        JourneyInstanceId instanceId,
        Type stateType,
        object state,
        IReadOnlyDictionary<object, object> properties,
        bool completed = false)
    {
        var genericType = typeof(JourneyInstance<>).MakeGenericType(stateType);

        return (JourneyInstance)Activator.CreateInstance(
            genericType,
            stateProvider,
            instanceId,
            state,
            properties,
            completed)!;
    }

    public async Task CompleteAsync()
    {
        if (Completed)
        {
            return;
        }

        if (Deleted)
        {
            throw new InvalidOperationException("Instance has been deleted.");
        }

        await _stateProvider.CompleteInstanceAsync(InstanceId, StateType);
        Completed = true;
    }

    public async Task DeleteAsync()
    {
        if (Deleted)
        {
            return;
        }

        await _stateProvider.DeleteInstanceAsync(InstanceId, StateType);
        Deleted = true;
    }

    internal static bool IsJourneyInstanceType(Type type)
    {
        return type == typeof(JourneyInstance) ||
            (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(JourneyInstance<>));
    }

    protected async Task UpdateStateAsync(object state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (state.GetType() != StateType)
        {
            throw new ArgumentException($"State must be type: '{StateType.FullName}'.", nameof(state));
        }

        if (Completed)
        {
            throw new InvalidOperationException("Instance has been completed.");
        }

        if (Deleted)
        {
            throw new InvalidOperationException("Instance has been deleted.");
        }

        await _stateProvider.UpdateInstanceStateAsync(InstanceId, StateType, state);
        State = state;
    }
}

public sealed class JourneyInstance<TState> : JourneyInstance
{
    public JourneyInstance(
        IUserInstanceStateProvider stateProvider,
        JourneyInstanceId instanceId,
        TState state,
        IReadOnlyDictionary<object, object> properties,
        bool completed = false)
        : base(stateProvider, instanceId, typeof(TState), state!, properties, completed)
    {
    }

    public new TState State => (TState)base.State;

    public async Task UpdateStateAsync(TState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        await UpdateStateAsync((object)state);
    }

    public async Task UpdateStateAsync(Action<TState> update)
    {
        update(State);
        await UpdateStateAsync(State);
    }

    public async Task UpdateStateAsync(Func<TState, Task> update)
    {
        await update(State);
        await UpdateStateAsync(State);
    }

    public async Task UpdateStateAsync(Func<TState, TState> update)
    {
        var newState = update(State);
        await UpdateStateAsync(newState);
    }
}

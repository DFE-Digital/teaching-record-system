using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.WebCommon.FormFlow.State;

namespace TeachingRecordSystem.WebCommon.FormFlow;

public class JourneyInstanceProvider(
    IUserInstanceStateProvider stateProvider,
    IOptions<FormFlowOptions> optionsAccessor)
{
    public Task<JourneyInstance> CreateInstanceAsync(
        ActionContext actionContext,
        Func<JourneyInstanceId, object> createState)
    {
        return CreateInstanceAsync(actionContext, id => Task.FromResult(createState(id)));
    }

    public async Task<JourneyInstance> CreateInstanceAsync(
        ActionContext actionContext,
        Func<JourneyInstanceId, Task<object>> createStateAsync)
    {
        ArgumentNullException.ThrowIfNull(createStateAsync);

        var journeyDescriptor = ResolveJourneyDescriptor(actionContext)!;

        var valueProvider = CreateValueProvider(actionContext);

        var instanceId = JourneyInstanceId.Create(
            journeyDescriptor,
            valueProvider);

        var state = await createStateAsync(instanceId);
        ThrowIfStateTypeIncompatible(state.GetType(), journeyDescriptor);

        if (await stateProvider.GetInstanceAsync(instanceId, journeyDescriptor.StateType) != null)
        {
            throw new InvalidOperationException("Instance already exists with this ID.");
        }

        var instance = await stateProvider.CreateInstanceAsync(
            instanceId,
            journeyDescriptor.StateType,
            state);

        EnsureActionContext(instance, actionContext);

        return instance;
    }

    public async Task<JourneyInstance<TState>> CreateInstanceAsync<TState>(
        ActionContext actionContext,
        Func<JourneyInstanceId, Task<TState>> createStateAsync)
        where TState : notnull
    {
        ArgumentNullException.ThrowIfNull(createStateAsync);

        var journeyDescriptor = ResolveJourneyDescriptor(actionContext)!;

        ThrowIfStateTypeIncompatible(typeof(TState), journeyDescriptor);

        var valueProvider = CreateValueProvider(actionContext);

        var instanceId = JourneyInstanceId.Create(
            journeyDescriptor,
            valueProvider);

        var state = await createStateAsync(instanceId);

        if (await stateProvider.GetInstanceAsync(instanceId, journeyDescriptor.StateType) != null)
        {
            throw new InvalidOperationException("Instance already exists with this ID.");
        }

        var instance = (JourneyInstance<TState>)await stateProvider.CreateInstanceAsync(
            instanceId,
            journeyDescriptor.StateType,
            state);

        EnsureActionContext(instance, actionContext);

        return instance;
    }

    public async Task<JourneyInstance?> GetInstanceAsync(ActionContext actionContext)
    {
        ArgumentNullException.ThrowIfNull(actionContext);

        // Throw if JourneyDescriptor is missing
        ResolveJourneyDescriptor(actionContext);

        return await ResolveCurrentInstanceAsync(actionContext);
    }

    public async Task<JourneyInstance<TState>?> GetInstanceAsync<TState>(ActionContext actionContext)
    {
        ArgumentNullException.ThrowIfNull(actionContext);

        // Throw if JourneyDescriptor is missing
        ResolveJourneyDescriptor(actionContext);

        var instance = await ResolveCurrentInstanceAsync(actionContext);

        if (instance is not null)
        {
            ThrowIfStateTypeIncompatible(typeof(TState), instance.StateType);

            return (JourneyInstance<TState>)instance;
        }
        else
        {
            return null;
        }
    }

    public async Task<JourneyInstance> GetOrCreateInstanceAsync(
        ActionContext actionContext,
        Func<JourneyInstanceId, object> createState)
    {
        ArgumentNullException.ThrowIfNull(actionContext);
        ArgumentNullException.ThrowIfNull(createState);

        var instance = await ResolveCurrentInstanceAsync(actionContext);

        if (instance is not null)
        {
            return instance;
        }

        return await CreateInstanceAsync(actionContext, id => Task.FromResult(createState(id)));
    }

    public Task<JourneyInstance<TState>> GetOrCreateInstanceAsync<TState>(ActionContext actionContext)
        where TState : notnull, new()
    {
        return GetOrCreateInstanceAsync(actionContext, _ => new TState());
    }

    public async Task<JourneyInstance<TState>> GetOrCreateInstanceAsync<TState>(
        ActionContext actionContext,
        Func<JourneyInstanceId, TState> createState)
        where TState : notnull
    {
        ArgumentNullException.ThrowIfNull(actionContext);
        ArgumentNullException.ThrowIfNull(createState);

        var journeyDescriptor = ResolveJourneyDescriptor(actionContext)!;

        ThrowIfStateTypeIncompatible(typeof(TState), journeyDescriptor);

        var instance = await ResolveCurrentInstanceAsync(actionContext);

        if (instance is not null)
        {
            return (JourneyInstance<TState>)instance;
        }

        return await CreateInstanceAsync(actionContext, id => Task.FromResult(createState(id)));
    }

    public async Task<JourneyInstance> GetOrCreateInstanceAsync(
        ActionContext actionContext,
        Func<JourneyInstanceId, Task<object>> createState)
    {
        ArgumentNullException.ThrowIfNull(actionContext);
        ArgumentNullException.ThrowIfNull(createState);

        var journeyDescriptor = ResolveJourneyDescriptor(actionContext)!;

        var instance = await ResolveCurrentInstanceAsync(actionContext);

        if (instance is not null)
        {
            return instance;
        }

        var valueProvider = CreateValueProvider(actionContext);

        var instanceId = JourneyInstanceId.Create(
            journeyDescriptor,
            valueProvider);

        var newState = await createState(instanceId);

        ThrowIfStateTypeIncompatible(newState.GetType(), journeyDescriptor);

        return await CreateInstanceAsync(actionContext, _ => Task.FromResult(newState));
    }

    public async Task<JourneyInstance<TState>> GetOrCreateInstanceAsync<TState>(
        ActionContext actionContext,
        Func<JourneyInstanceId, Task<TState>> createState)
        where TState : notnull
    {
        ArgumentNullException.ThrowIfNull(actionContext);
        ArgumentNullException.ThrowIfNull(createState);

        var journeyDescriptor = ResolveJourneyDescriptor(actionContext)!;

        ThrowIfStateTypeIncompatible(typeof(TState), journeyDescriptor);

        var instance = await ResolveCurrentInstanceAsync(actionContext);

        if (instance is not null)
        {
            return (JourneyInstance<TState>)instance;
        }

        var valueProvider = CreateValueProvider(actionContext);

        var instanceId = JourneyInstanceId.Create(
            journeyDescriptor,
            valueProvider);

        var newState = await createState(instanceId);

        return await CreateInstanceAsync(actionContext, _ => Task.FromResult(newState));
    }

    public bool IsCurrentInstance(ActionContext actionContext, JourneyInstance instance)
    {
        ArgumentNullException.ThrowIfNull(actionContext);
        ArgumentNullException.ThrowIfNull(instance);

        return IsCurrentInstance(actionContext, instance.InstanceId);
    }

    public bool IsCurrentInstance(ActionContext actionContext, JourneyInstanceId instanceId)
    {
        ArgumentNullException.ThrowIfNull(actionContext);

        var journeyDescriptor = ResolveJourneyDescriptor(actionContext, throwIfNotFound: true)!;
        var currentInstanceId = ResolveCurrentInstanceId(actionContext, journeyDescriptor);
        return currentInstanceId == instanceId;
    }

    internal JourneyDescriptor? ResolveJourneyDescriptor(
        ActionContext actionContext,
        bool throwIfNotFound = true)
    {
        var actionJourneyMetadata = actionContext.GetActionJourneyMetadata();
        if (actionJourneyMetadata == null)
        {
            if (throwIfNotFound)
            {
                throw new InvalidOperationException("No journey metadata found on action.");
            }
            else
            {
                return null;
            }
        }

        var journeyDescriptor = optionsAccessor.Value.JourneyRegistry.GetJourneyByName(actionJourneyMetadata.JourneyName);
        if (journeyDescriptor is null)
        {
            throw new InvalidOperationException($"No journey named '{actionJourneyMetadata.JourneyName}' found in JourneyRegistry.");
        }

        return journeyDescriptor;
    }

    internal async Task<JourneyInstance?> ResolveCurrentInstanceAsync(ActionContext actionContext)
    {
        ArgumentNullException.ThrowIfNull(actionContext);

        // If we've already created a JourneyInstance for this request, use that
        if (actionContext.HttpContext.Items.TryGetValue(typeof(JourneyInstance), out var currentInstanceObj) && currentInstanceObj is not null)
        {
            return (JourneyInstance)currentInstanceObj;
        }

        var journeyDescriptor = ResolveJourneyDescriptor(actionContext, throwIfNotFound: false);
        if (journeyDescriptor == null)
        {
            return null;
        }

        var instanceId = ResolveCurrentInstanceId(actionContext, journeyDescriptor);
        if (instanceId is null)
        {
            return null;
        }

        var persistedInstance = await stateProvider.GetInstanceAsync(instanceId.Value, journeyDescriptor.StateType);
        if (persistedInstance == null)
        {
            return null;
        }

        if (persistedInstance.JourneyName != journeyDescriptor.JourneyName)
        {
            return null;
        }

        if (persistedInstance.StateType != journeyDescriptor.StateType)
        {
            return null;
        }

        // Protect against stateProvider handing back a deleted instance
        if (persistedInstance.Deleted)
        {
            return null;
        }

        EnsureActionContext(persistedInstance, actionContext);

        actionContext.HttpContext.Items.TryAdd(typeof(JourneyInstance), persistedInstance);

        // There's a race here; another thread could resolve an instance and beat us to adding it to cache.
        // Ensure we return the cached instance.
        return (JourneyInstance)actionContext.HttpContext.Items[typeof(JourneyInstance)]!;
    }

    private static void EnsureActionContext(JourneyInstance instance, ActionContext actionContext)
    {
        if (instance.Properties.ContainsKey(typeof(ActionContext)))
        {
            return;
        }

        instance.Properties.Add(typeof(ActionContext), actionContext);
    }

    private JourneyInstanceId? ResolveCurrentInstanceId(ActionContext actionContext, JourneyDescriptor journeyDescriptor)
    {
        ArgumentNullException.ThrowIfNull(actionContext);

        var valueProvider = CreateValueProvider(actionContext);
        return JourneyInstanceId.TryResolve(journeyDescriptor, valueProvider, out var instanceId) ? instanceId : null;
    }

    private static void ThrowIfStateTypeIncompatible(Type stateType, JourneyDescriptor journeyDescriptor) =>
        ThrowIfStateTypeIncompatible(stateType, journeyDescriptor.StateType);

    private static void ThrowIfStateTypeIncompatible(Type stateType, Type instanceStateType)
    {
        if (stateType != instanceStateType)
        {
            throw new InvalidOperationException(
                $"{stateType.FullName} is not compatible with the journey's state type ({instanceStateType.FullName}).");
        }
    }

    private IValueProvider CreateValueProvider(ActionContext actionContext)
    {
        if (actionContext.HttpContext.Items.TryGetValue(typeof(ValueProviderCacheEntry), out var cacheEntry) && cacheEntry is not null)
        {
            return ((ValueProviderCacheEntry)cacheEntry).ValueProvider;
        }

        var valueProviders = new List<IValueProvider>();

        foreach (var valueProviderFactory in optionsAccessor.Value.ValueProviderFactories)
        {
            var ctx = new ValueProviderFactoryContext(actionContext);

            // All the in-box implementations of IValueProviderFactory complete synchronously
            // and making this method async forces the entire API to be sync.
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            valueProviderFactory.CreateValueProviderAsync(ctx).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits

            valueProviders.AddRange(ctx.ValueProviders);
        }

        var valueProvider = new CompositeValueProvider(valueProviders);

        actionContext.HttpContext.Items.TryAdd(
            typeof(ValueProviderCacheEntry),
            new ValueProviderCacheEntry(valueProvider));

        return valueProvider;
    }

    private class ValueProviderCacheEntry
    {
        public ValueProviderCacheEntry(IValueProvider valueProvider)
        {
            ValueProvider = valueProvider;
        }

        public IValueProvider ValueProvider { get; }
    }
}

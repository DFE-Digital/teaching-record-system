using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.FormFlow;
using TeachingRecordSystem.FormFlow.State;

namespace TeachingRecordSystem.AuthorizeAccessToATeacherRecord.Infrastructure.FormFlow;

public class SignInJourneyHelper
{
    private readonly IUserInstanceStateProvider _userInstanceStateProvider;
    private readonly JourneyDescriptor _journeyDescriptor;

    public SignInJourneyHelper(IUserInstanceStateProvider userInstanceStateProvider, IOptions<FormFlowOptions> formFlowOptionsAccessor)
    {
        _userInstanceStateProvider = userInstanceStateProvider;

        _journeyDescriptor = formFlowOptionsAccessor.Value.JourneyRegistry.GetJourneyByName(SignInJourneyState.JourneyName) ??
            throw new InvalidOperationException($"Cannot find {SignInJourneyState.JourneyName} journey.");
    }

    public async Task<JourneyInstance<SignInJourneyState>?> GetInstanceAsync(HttpContext httpContext, JourneyInstanceId? instanceIdHint = null)
    {
        if (httpContext.Items.TryGetValue(typeof(JourneyInstance), out var journeyInstanceObj) &&
            journeyInstanceObj is JourneyInstance<SignInJourneyState> instance)
        {
            return instance;
        }

        var valueProvider = CreateValueProvider(httpContext);

        if (!JourneyInstanceId.TryResolve(_journeyDescriptor, valueProvider, out var instanceId) && instanceIdHint is null)
        {
            return null;
        }

        if (await _userInstanceStateProvider.GetInstanceAsync(instanceIdHint ?? instanceId, typeof(SignInJourneyState))
            is not JourneyInstance<SignInJourneyState> persistedInstance)
        {
            return null;
        }

        httpContext.Items[typeof(JourneyInstance)] = persistedInstance;

        return persistedInstance;
    }

    public async Task<JourneyInstance<SignInJourneyState>> GetOrCreateInstanceAsync(
        HttpContext httpContext,
        Func<SignInJourneyState> createState,
        Action<SignInJourneyState> updateState)
    {
        var existingInstance = await GetInstanceAsync(httpContext);

        if (existingInstance is not null)
        {
            await existingInstance.UpdateStateAsync(updateState);
            return existingInstance;
        }

        var valueProvider = CreateValueProvider(httpContext);
        var instanceId = JourneyInstanceId.Create(_journeyDescriptor, valueProvider);

        var newState = createState();
        var instance = (JourneyInstance<SignInJourneyState>)await _userInstanceStateProvider.CreateInstanceAsync(instanceId, typeof(SignInJourneyState), newState, properties: null);

        httpContext.Items[typeof(JourneyInstance)] = instance;

        return instance;
    }

    private static IValueProvider CreateValueProvider(HttpContext httpContext) =>
        new QueryStringValueProvider(BindingSource.Query, httpContext.Request.Query, culture: null);
}

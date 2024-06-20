using Microsoft.AspNetCore.Mvc.ModelBinding;
using TeachingRecordSystem.UiCommon.FormFlow;
using TeachingRecordSystem.UiCommon.FormFlow.State;

namespace TeachingRecordSystem.AuthorizeAccess.Infrastructure.FormFlow;

public static class Extensions
{
    public static async Task<JourneyInstance<SignInJourneyState>?> GetSignInJourneyInstanceAsync(
        this IUserInstanceStateProvider userInstanceStateProvider,
        HttpContext httpContext,
        JourneyInstanceId? instanceIdHint = null)
    {
        if (httpContext.Items.TryGetValue(typeof(JourneyInstance), out var journeyInstanceObj) &&
            journeyInstanceObj is JourneyInstance<SignInJourneyState> instance)
        {
            return instance;
        }
        var valueProvider = CreateValueProvider(httpContext);

        if (!JourneyInstanceId.TryResolve(SignInJourneyState.JourneyDescriptor, valueProvider, out var instanceId) && instanceIdHint is null)
        {
            return null;
        }

        if (await userInstanceStateProvider.GetInstanceAsync(instanceIdHint ?? instanceId, typeof(SignInJourneyState))
            is not JourneyInstance<SignInJourneyState> persistedInstance)
        {
            return null;
        }

        httpContext.Items[typeof(JourneyInstance)] = persistedInstance;

        return persistedInstance;
    }

    public static async Task<JourneyInstance<SignInJourneyState>> GetOrCreateSignInJourneyInstanceAsync(
        this IUserInstanceStateProvider userInstanceStateProvider,
        HttpContext httpContext,
        Func<SignInJourneyState> createState,
        Action<SignInJourneyState> updateState)
    {
        var existingInstance = await GetSignInJourneyInstanceAsync(userInstanceStateProvider, httpContext);

        if (existingInstance is not null)
        {
            await existingInstance.UpdateStateAsync(updateState);
            return existingInstance;
        }

        var valueProvider = CreateValueProvider(httpContext);
        var instanceId = JourneyInstanceId.Create(SignInJourneyState.JourneyDescriptor, valueProvider);

        var newState = createState();
        var instance = (JourneyInstance<SignInJourneyState>)await userInstanceStateProvider.CreateInstanceAsync(instanceId, typeof(SignInJourneyState), newState, properties: null);

        httpContext.Items[typeof(JourneyInstance)] = instance;

        return instance;
    }

    private static IValueProvider CreateValueProvider(HttpContext httpContext) =>
        new QueryStringValueProvider(BindingSource.Query, httpContext.Request.Query, culture: null);
}

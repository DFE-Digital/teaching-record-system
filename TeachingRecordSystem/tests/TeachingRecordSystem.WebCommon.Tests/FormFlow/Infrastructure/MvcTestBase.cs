using Microsoft.Extensions.Primitives;
using TeachingRecordSystem.WebCommon.FormFlow;
using TeachingRecordSystem.WebCommon.FormFlow.State;

namespace TeachingRecordSystem.WebCommon.Tests.FormFlow.Infrastructure;

[Collection("Mvc")]
public abstract class MvcTestBase
{
    protected MvcTestBase(MvcTestFixture fixture)
    {
        Fixture = fixture;

        StateProvider.Clear();
    }

    protected MvcTestFixture Fixture { get; }

    protected HttpClient HttpClient => Fixture.HttpClient;

    protected InMemoryInstanceStateProvider StateProvider =>
        (InMemoryInstanceStateProvider)Fixture.Services.GetRequiredService<IUserInstanceStateProvider>();

    protected async Task<JourneyInstance<TState>> CreateInstanceAsync<TState>(
        string journeyName,
        IReadOnlyDictionary<string, StringValues> keys,
        TState state,
        IReadOnlyDictionary<object, object>? properties = null,
        string? uniqueKey = null)
        where TState : notnull
    {
        var routeValues = new RouteValueDictionary(keys);

        if (uniqueKey != null)
        {
            routeValues.Add(Constants.UniqueKeyQueryParameterName, uniqueKey);
        }

        var instanceId = new JourneyInstanceId(journeyName, keys);

        var instanceStateProvider = Fixture.Services.GetRequiredService<IUserInstanceStateProvider>();

        return (JourneyInstance<TState>)await instanceStateProvider.CreateInstanceAsync(
            instanceId,
            typeof(TState),
            state,
            properties);
    }
}

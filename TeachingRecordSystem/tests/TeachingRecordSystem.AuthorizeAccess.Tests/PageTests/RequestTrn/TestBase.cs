using FakeXrmEasy.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.UiCommon.FormFlow;
using TeachingRecordSystem.UiCommon.FormFlow.State;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests.RequestTrn;

public abstract class TestBase : IDisposable
{
    private readonly TestScopedServices _testServices;

    protected TestBase(HostFixture hostFixture)
    {
        HostFixture = hostFixture;

        _testServices = TestScopedServices.Reset();

        HttpClient = hostFixture.CreateClient(new()
        {
            AllowAutoRedirect = false
        });
    }

    public HostFixture HostFixture { get; }

    public CaptureEventObserver EventObserver => _testServices.EventObserver;

    public TestableClock Clock => _testServices.Clock;

    public HttpClient HttpClient { get; }

    public TestData TestData => HostFixture.Services.GetRequiredService<TestData>();

    public IXrmFakedContext XrmFakedContext => HostFixture.Services.GetRequiredService<IXrmFakedContext>();

    public async Task<JourneyInstance<RequestTrnJourneyState>> CreateJourneyInstance(RequestTrnJourneyState state)
    {
        await using var scope = HostFixture.Services.CreateAsyncScope();
        var stateProvider = scope.ServiceProvider.GetRequiredService<IUserInstanceStateProvider>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<FormFlowOptions>>();

        var journeyDescriptor = RequestTrnJourneyState.JourneyDescriptor;

        var keysDict = new Dictionary<string, StringValues>
        {
            { Constants.UniqueKeyQueryParameterName, new StringValues(Guid.NewGuid().ToString()) }
        };

        var instanceId = new JourneyInstanceId(journeyDescriptor.JourneyName, keysDict);

        var stateType = typeof(RequestTrnJourneyState);

        var instance = await stateProvider.CreateInstanceAsync(instanceId, stateType, state, properties: null);
        return (JourneyInstance<RequestTrnJourneyState>)instance;
    }

    public async Task<JourneyInstance<RequestTrnJourneyState>> ReloadJourneyInstance(JourneyInstance<RequestTrnJourneyState> journeyInstance)
    {
        await using var scope = HostFixture.Services.CreateAsyncScope();
        var stateProvider = scope.ServiceProvider.GetRequiredService<IUserInstanceStateProvider>();
        var reloadedInstance = await stateProvider.GetInstanceAsync(journeyInstance.InstanceId, typeof(RequestTrnJourneyState));
        return (JourneyInstance<RequestTrnJourneyState>)reloadedInstance!;
    }

    public RequestTrnJourneyState CreateNewState(string? email = null) => new RequestTrnJourneyState() { Email = email };

    public virtual void Dispose()
    {
    }

    public virtual async Task<T> WithDbContext<T>(Func<TrsDbContext, Task<T>> action)
    {
        var dbContextFactory = HostFixture.Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await action(dbContext);
    }

    public virtual Task WithDbContext(Func<TrsDbContext, Task> action) =>
        WithDbContext(async dbContext =>
        {
            await action(dbContext);
            return 0;
        });
}

using System.Reactive.Linq;
using FakeXrmEasy.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;
using TeachingRecordSystem.SupportUi.Tests.Infrastructure.Security;
using TeachingRecordSystem.UiCommon.FormFlow.State;

namespace TeachingRecordSystem.SupportUi.Tests;

public abstract class TestBase : IDisposable
{
    private readonly TestScopedServices _testServices;
    private readonly IDisposable _trsSyncSubscription;

    protected TestBase(HostFixture hostFixture)
    {
        HostFixture = hostFixture;

        _testServices = TestScopedServices.Reset();
        SetCurrentUser(TestUsers.Administrator);

        HttpClient = hostFixture.CreateClient(new()
        {
            AllowAutoRedirect = false
        });

        _trsSyncSubscription = hostFixture.Services.GetRequiredService<TrsDataSyncHelper>().GetSyncedEntitiesObservable()
            .Subscribe(onNext: static (object[] synced) =>
            {
                var events = synced.OfType<EventBase>();
                foreach (var e in events)
                {
                    TestScopedServices.GetCurrent().EventPublisher.PublishEvent(e);
                }
            });
    }

    public HostFixture HostFixture { get; }

    public CaptureEventPublisher EventPublisher => _testServices.EventPublisher;

    public Mock<IDataverseAdapter> DataverseAdapterMock => _testServices.DataverseAdapterMock;

    public TestableClock Clock => _testServices.Clock;

    public Mock<Services.AzureActiveDirectory.IAadUserService> AzureActiveDirectoryUserServiceMock => _testServices.AzureActiveDirectoryUserServiceMock;

    public HttpClient HttpClient { get; }

    public TestData TestData => HostFixture.Services.GetRequiredService<TestData>();

    public IXrmFakedContext XrmFakedContext => HostFixture.Services.GetRequiredService<IXrmFakedContext>();

    public async Task<JourneyInstance<TState>> CreateJourneyInstance<TState>(
            string journeyName,
            TState state,
            params KeyValuePair<string, object>[] keys)
        where TState : notnull
    {
        await using var scope = HostFixture.Services.CreateAsyncScope();
        var stateProvider = scope.ServiceProvider.GetRequiredService<IUserInstanceStateProvider>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<FormFlowOptions>>();

        var journeyDescriptor = options.Value.JourneyRegistry.GetJourneyByName(journeyName) ??
            throw new ArgumentException("Journey not found.", nameof(journeyName));

        var keysDict = keys.ToDictionary(k => k.Key, k => new StringValues(k.Value.ToString()));

        if (journeyDescriptor.AppendUniqueKey)
        {
            keysDict.Add(Constants.UniqueKeyQueryParameterName, new StringValues(Guid.NewGuid().ToString()));
        }

        var instanceId = new JourneyInstanceId(journeyDescriptor.JourneyName, keysDict);

        var stateType = typeof(TState);

        var instance = await stateProvider.CreateInstanceAsync(instanceId, stateType, state, properties: null);
        return (JourneyInstance<TState>)instance;
    }

    public async Task<JourneyInstance<TState>> ReloadJourneyInstance<TState>(JourneyInstance<TState> journeyInstance)
    {
        await using var scope = HostFixture.Services.CreateAsyncScope();
        var stateProvider = scope.ServiceProvider.GetRequiredService<IUserInstanceStateProvider>();
        var reloadedInstance = await stateProvider.GetInstanceAsync(journeyInstance.InstanceId, typeof(TState));
        return (JourneyInstance<TState>)reloadedInstance!;
    }

    public virtual void Dispose()
    {
        _trsSyncSubscription.Dispose();
    }

    protected Guid GetCurrentUserId()
    {
        var currentUserProvider = HostFixture.Services.GetRequiredService<CurrentUserProvider>();
        return (currentUserProvider.CurrentUser ?? throw new InvalidOperationException("No current user set.")).UserId;
    }

    protected void SetCurrentUser(User user)
    {
        var currentUserProvider = HostFixture.Services.GetRequiredService<CurrentUserProvider>();
        currentUserProvider.CurrentUser = user;
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

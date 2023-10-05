using FakeXrmEasy.Abstractions;
using FormFlow;
using FormFlow.State;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.SupportUi.Tests.Infrastructure;
using TeachingRecordSystem.SupportUi.Tests.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Tests;

public abstract class TestBase
{
    private readonly TestScopedServices _testServices;

    protected TestBase(HostFixture hostFixture)
    {
        HostFixture = hostFixture;

        HostFixture.EventObserver.Init();
        _testServices = TestScopedServices.Reset();
        SetCurrentUser(TestUsers.Administrator);

        HttpClient = hostFixture.CreateClient(new()
        {
            AllowAutoRedirect = false
        });
    }

    public HostFixture HostFixture { get; }

    public CaptureEventObserver EventObserver => HostFixture.EventObserver;

    public Mock<IDataverseAdapter> DataverseAdapterMock => _testServices.DataverseAdapterMock;

    public TestableClock Clock => _testServices.Clock;

    public Mock<Services.AzureActiveDirectory.IUserService> AzureActiveDirectoryUserServiceMock => _testServices.AzureActiveDirectoryUserServiceMock;

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

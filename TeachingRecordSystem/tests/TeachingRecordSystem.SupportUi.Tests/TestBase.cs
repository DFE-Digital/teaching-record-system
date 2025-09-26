using FakeXrmEasy.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.Core.Services.TrsDataSync;
using TeachingRecordSystem.SupportUi.Tests.Infrastructure.Security;
using TeachingRecordSystem.TestCommon.Infrastructure;
using TeachingRecordSystem.WebCommon.FormFlow.State;

namespace TeachingRecordSystem.SupportUi.Tests;

public abstract class TestBase : IDisposable
{
    private readonly TestScopedServices _testServices;
    private readonly IDisposable _trsSyncSubscription;

    protected TestBase(HostFixture hostFixture)
    {
        HostFixture = hostFixture;

        _testServices = TestScopedServices.Reset(hostFixture.Services);
        SetCurrentUser(TestUsers.GetUser(UserRoles.Administrator));

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
                    TestScopedServices.GetCurrent().EventObserver.OnEventCreated(e);
                }
            });
    }

    public HostFixture HostFixture { get; }

    public CaptureEventObserver EventPublisher => _testServices.EventObserver;

    public TestableClock Clock => _testServices.Clock;

    public Mock<Services.AzureActiveDirectory.IAadUserService> AzureActiveDirectoryUserServiceMock => _testServices.AzureActiveDirectoryUserServiceMock;

    public HttpClient HttpClient { get; }

    public TestData TestData => HostFixture.Services.GetRequiredService<TestData>();

    public TestUsers TestUsers => HostFixture.Services.GetRequiredService<TestUsers>();

    public IXrmFakedContext XrmFakedContext => HostFixture.Services.GetRequiredService<IXrmFakedContext>();

    public TestableFeatureProvider FeatureProvider => _testServices.FeatureProvider;

    public ReferenceDataCache ReferenceDataCache => HostFixture.Services.GetRequiredService<ReferenceDataCache>();

    public Mock<IFileService> FileServiceMock => _testServices.BlobStorageFileServiceMock;

    public Mock<IGetAnIdentityApiClient> GetAnIdentityApiClientMock => _testServices.GetAnIdentityApiClientMock;

    public TrnRequestOptions TrnRequestOptions => _testServices.TrnRequestOptions;

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

    public async Task<TState> CreateJourneyStateWithFactory<TFactory, TState>(Func<TFactory, Task<TState>> createState)
        where TFactory : IJourneyStateFactory<TState>
    {
        await using var scope = HostFixture.Services.CreateAsyncScope();
        var factory = ActivatorUtilities.CreateInstance<TFactory>(scope.ServiceProvider);
        return await createState(factory);
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

    protected Task<(TestData.CreatePersonResult, Alert)> CreatePersonWithOpenAlert(bool populateOptional = true, EventModels.RaisedByUserInfo? createdByUser = null)
    {
        return CreatePersonWithAlert(isOpenAlert: true, populateOptional: populateOptional, createdByUser: createdByUser);
    }

    protected Task<(TestData.CreatePersonResult, Alert)> CreatePersonWithClosedAlert(bool populateOptional = true, EventModels.RaisedByUserInfo? createdByUser = null)
    {
        return CreatePersonWithAlert(isOpenAlert: false, populateOptional: populateOptional, createdByUser: createdByUser);
    }

    protected async Task<(TestData.CreatePersonResult, Alert)> CreatePersonWithAlert(bool isOpenAlert, bool populateOptional = true, EventModels.RaisedByUserInfo? createdByUser = null)
    {
        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a =>
            {
                a.WithStartDate(Clock.Today.AddDays(-30));
                a.WithEndDate(isOpenAlert ? null : Clock.Today.AddDays(-1));
                a.WithExternalLink(populateOptional ? TestData.GenerateUrl() : null);
                if (createdByUser is not null)
                {
                    a.WithCreatedByUser(createdByUser);
                }
            }));

        return (person, person.Alerts.Single());
    }

    protected static HttpContent CreateEvidenceFileBinaryContent(byte[]? content = null)
    {
        var byteArrayContent = new ByteArrayContent(content ?? []);
        byteArrayContent.Headers.Add("Content-Type", "application/octet-stream");
        return byteArrayContent;
    }

    public static TheoryData<HttpMethod> HttpMethods(TestHttpMethods httpMethods)
    {
        var data = new TheoryData<HttpMethod>();

        foreach (var httpMethod in httpMethods.AsHttpMethods())
        {
            data.Add(httpMethod);
        }

        return data;
    }

    public static IEnumerable<object?[]> AllCombinationsOf(object? param1, object? param2)
    {
        foreach (var item1 in ToObjectEnumerable(param1))
        {
            foreach (var item2 in ToObjectEnumerable(param2))
            {
                yield return [item1, item2];
            }
        }
    }

    public static IEnumerable<object?[]> AllCombinationsOf(object? param1, object? param2, object? param3)
    {
        foreach (var item1 in ToObjectEnumerable(param1))
        {
            foreach (var item2 in ToObjectEnumerable(param2))
            {
                foreach (var item3 in ToObjectEnumerable(param3))
                {
                    yield return [item1, item2, item3];
                }
            }
        }
    }

    public static IEnumerable<object?[]> AllCombinationsOf(object? param1, object? param2, object? param3, object? param4)
    {
        foreach (var item1 in ToObjectEnumerable(param1))
        {
            foreach (var item2 in ToObjectEnumerable(param2))
            {
                foreach (var item3 in ToObjectEnumerable(param3))
                {
                    foreach (var item4 in ToObjectEnumerable(param4))
                    {
                        yield return [item1, item2, item3, item4];
                    }
                }
            }
        }
    }

    private static IEnumerable<object?> ToObjectEnumerable(object? obj) => obj switch
    {
        TestHttpMethods httpMethod => httpMethod.AsHttpMethods(),
        string s => [s],
        IEnumerable<object?> genericList => genericList,
        System.Collections.IEnumerable list => list.Cast<object?>(),
        _ => [obj]
    };
}

public enum TestHttpMethods
{
    None = 0,
    Get = 1 << 1,
    Post = 1 << 2,
    GetAndPost = Get | Post
}

public static class TestHttpMethodExtensions
{
    public static IEnumerable<HttpMethod> AsHttpMethods(this TestHttpMethods httpMethod)
    {
        if ((httpMethod & TestHttpMethods.Get) > 0)
        {
            yield return HttpMethod.Get;
        }

        if ((httpMethod & TestHttpMethods.Post) > 0)
        {
            yield return HttpMethod.Post;
        }
    }
}


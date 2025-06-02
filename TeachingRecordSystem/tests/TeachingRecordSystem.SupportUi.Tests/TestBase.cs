using System.Reactive.Linq;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using FakeXrmEasy.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
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

    public Mock<IDataverseAdapter> DataverseAdapterMock => _testServices.DataverseAdapterMock;

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

    protected T GetChildElementOfTestId<T>(IHtmlDocument doc, string testId, string childSelector) where T : IElement
    {
        var parent = doc.GetElementByTestId(testId);
        Assert.NotNull(parent);
        var child = parent.QuerySelector(childSelector);
        Assert.NotNull(child);
        Assert.IsAssignableFrom<T>(child);
        return (T)child;
    }

    protected IEnumerable<T> GetChildElementsOfTestId<T>(IHtmlDocument doc, string testId, string childSelector) where T : IElement
    {
        var parent = doc.GetElementByTestId(testId);
        Assert.NotNull(parent);
        var children = parent.QuerySelectorAll(childSelector);
        Assert.All(children, c => Assert.IsAssignableFrom<T>(c));
        return children.Cast<T>();
    }

    protected string GetHiddenInputValue(IHtmlDocument html, string name)
    {
        var element = html.QuerySelector($@"input[type=""hidden""][name=""{name}""]");
        var input = Assert.IsAssignableFrom<IHtmlInputElement>(element);

        return input.Value;
    }
}

using System.Transactions;
using FakeXrmEasy.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.SupportUi.Tests.Infrastructure.Security;
using TeachingRecordSystem.TestCommon.Infrastructure;
using TeachingRecordSystem.WebCommon.FormFlow.State;

namespace TeachingRecordSystem.SupportUi.Tests;

[SharedDependenciesDataSource]
public abstract class TestBase(HostFixture hostFixture)
{
    [Before(Test)]
    public void TestSetup(TestContext context)
    {
        var transactionScope = new TransactionScope(
            TransactionScopeOption.RequiresNew,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);
        context.ObjectBag[nameof(TransactionScope)] = transactionScope;

        var testScopedServices = TestScopedServices.Reset(HostFixture.Services);
        testScopedServices.EventObserver.Clear();

        SetCurrentUser(Setup.AdminUser);

        context.AddAsyncLocalValues();
    }

    [After(Test)]
    public void TestTeardown(TestContext context)
    {
        if (context.ObjectBag.TryGetValue(nameof(TransactionScope), out var txnObj) && txnObj is TransactionScope txn)
        {
            txn.Dispose();
        }
    }

    protected HostFixture HostFixture { get; } = hostFixture;

    protected CaptureEventObserver EventObserver => TestScopedServices.GetCurrent().EventObserver;

    protected TestableClock Clock => TestScopedServices.GetCurrent().Clock;

    protected Mock<Services.AzureActiveDirectory.IAadUserService> AzureActiveDirectoryUserServiceMock =>
        TestScopedServices.GetCurrent().AzureActiveDirectoryUserServiceMock;

    protected HttpClient HttpClient { get; } = hostFixture.CreateClient(new()
    {
        AllowAutoRedirect = false
    });

    protected TestData TestData => HostFixture.Services.GetRequiredService<TestData>();

    protected IXrmFakedContext XrmFakedContext => HostFixture.Services.GetRequiredService<IXrmFakedContext>();

    protected TestableFeatureProvider FeatureProvider => TestScopedServices.GetCurrent().FeatureProvider;

    protected ReferenceDataCache ReferenceDataCache => HostFixture.Services.GetRequiredService<ReferenceDataCache>();

    protected Mock<IFileService> FileServiceMock => TestScopedServices.GetCurrent().BlobStorageFileServiceMock;

    protected Mock<IGetAnIdentityApiClient> GetAnIdentityApiClientMock => TestScopedServices.GetCurrent().GetAnIdentityApiClientMock;

    protected TrnRequestOptions TrnRequestOptions => TestScopedServices.GetCurrent().TrnRequestOptions;

    protected async Task<JourneyInstance<TState>> CreateJourneyInstance<TState>(
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

    protected async Task<TState> CreateJourneyStateWithFactory<TFactory, TState>(Func<TFactory, Task<TState>> createState)
        where TFactory : IJourneyStateFactory<TState>
    {
        await using var scope = HostFixture.Services.CreateAsyncScope();
        var factory = ActivatorUtilities.CreateInstance<TFactory>(scope.ServiceProvider);
        return await createState(factory);
    }

    protected async Task<JourneyInstance<TState>> ReloadJourneyInstance<TState>(JourneyInstance<TState> journeyInstance)
    {
        await using var scope = HostFixture.Services.CreateAsyncScope();
        var stateProvider = scope.ServiceProvider.GetRequiredService<IUserInstanceStateProvider>();
        var reloadedInstance = await stateProvider.GetInstanceAsync(journeyInstance.InstanceId, typeof(TState));
        return (JourneyInstance<TState>)reloadedInstance!;
    }

    protected Guid GetCurrentUserId() =>
        TestScopedServices.GetCurrent().CurrentUserProvider.CurrentUser?.UserId ?? throw new InvalidOperationException("No current user set.");

    protected void SetCurrentUser(User user) =>
        TestScopedServices.GetCurrent().CurrentUserProvider.CurrentUser = user;

    protected async Task<T> WithDbContext<T>(Func<TrsDbContext, Task<T>> action)
    {
        var dbContextFactory = HostFixture.Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await action(dbContext);
    }

    protected Task WithDbContext(Func<TrsDbContext, Task> action) =>
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
}


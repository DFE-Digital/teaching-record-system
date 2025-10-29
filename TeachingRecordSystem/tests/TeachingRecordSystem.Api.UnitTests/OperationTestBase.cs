using System.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.TestCommon.Infrastructure;

namespace TeachingRecordSystem.Api.UnitTests;

public abstract class OperationTestBase
{
    [SharedDependenciesDataSource]
    public required IServiceProvider Services { get; init; }

    [Before(Test)]
    public void TestSetup(TestContext context)
    {
        var transactionScope = new TransactionScope(
            TransactionScopeOption.RequiresNew,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);
        context.ObjectBag[nameof(TransactionScope)] = transactionScope;

        var testScopedServices = TestScopedServices.Reset(Services);
        testScopedServices.EventObserver.Clear();

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

    protected TestableClock Clock => TestScopedServices.GetCurrent().Clock;

    protected ICurrentUserProvider CurrentUserProvider => Services.GetRequiredService<ICurrentUserProvider>();

    protected DbHelper DbHelper => Services.GetRequiredService<DbHelper>();

    protected IDbContextFactory<TrsDbContext> DbContextFactory => Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    protected TestData TestData => Services.GetRequiredService<TestData>();

    protected Mock<IGetAnIdentityApiClient> GetAnIdentityApiClientMock => Mock.Get(Services.GetRequiredService<IGetAnIdentityApiClient>());

    protected CaptureEventObserver EventObserver => TestScopedServices.GetCurrent().EventObserver;

    protected TestableFeatureProvider FeatureProvider => (TestableFeatureProvider)Services.GetRequiredService<IFeatureProvider>();

    protected TrnRequestOptions TrnRequestOptions => Services.GetRequiredService<IOptions<TrnRequestOptions>>().Value;

    protected IFileService FileService => Services.GetRequiredService<IFileService>();

    protected T AssertSuccess<T>(ApiResult<T> result) where T : notnull
    {
        if (result.IsError)
        {
            Assert.False(result.IsError, $"Result is not in a Success state (got error: {result.GetError().ErrorCode})");
        }

        return result.GetSuccess();
    }

    protected ApiError AssertError<T>(ApiResult<T> result, int expectedErrorCode) where T : notnull
    {
        Assert.True(result.IsError, "Result is not in a Error state.");
        var error = result.GetError();
        Assert.Equal(expectedErrorCode, error.ErrorCode);
        return error;
    }

    protected async Task<ApiResult<TResult>> ExecuteCommandAsync<TResult>(ICommand<TResult> command)
        where TResult : notnull
    {
        var serviceScopeFactory = Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = serviceScopeFactory.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();
        var result = await dispatcher.DispatchAsync(command);

        await TestScopedServices.GetCurrent().BackgroundJobScheduler.ExecuteDeferredJobsAsync();

        return result;
    }

    protected Task<T> WithDbContextAsync<T>(Func<TrsDbContext, Task<T>> action) =>
        DbContextFactory.WithDbContextAsync(action);

    protected Task WithDbContextAsync(Func<TrsDbContext, Task> action) =>
        DbContextFactory.WithDbContextAsync(action);
}

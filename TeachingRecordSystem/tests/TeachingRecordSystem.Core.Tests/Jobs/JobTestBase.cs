using Microsoft.Testing.Platform.Services;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection(nameof(DisableParallelization)), ClearDbBeforeTest]
public abstract class JobTestBase(JobFixture fixture)
{
    protected TestableClock Clock => (TestableClock)fixture.Services.GetRequiredService<IClock>();

    protected IDbContextFactory<TrsDbContext> DbContextFactory => fixture.Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    protected TestData TestData => fixture.Services.GetRequiredService<TestData>();

    protected Task<T> WithDbContextAsync<T>(Func<TrsDbContext, Task<T>> action) =>
        DbContextFactory.WithDbContextAsync(action);

    protected Task WithDbContextAsync(Func<TrsDbContext, Task> action) =>
        DbContextFactory.WithDbContextAsync(action);

    protected Task WithServiceAsync<TService>(Func<TService, Task> action, params object[] arguments) where TService : notnull =>
        fixture.WithServiceAsync(action, arguments);

    protected Task<TResult> WithServiceAsync<TService, TResult>(Func<TService, Task<TResult>> action, params object[] arguments) where TService : notnull =>
        fixture.WithServiceAsync(action, arguments);
}

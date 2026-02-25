using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection(nameof(DisableParallelization)), ClearDbBeforeTest]
public abstract class JobTestBase
{
    private readonly JobFixture _fixture;

    protected JobTestBase(JobFixture fixture)
    {
        _fixture = fixture;
        TestScopedServices.Reset();
    }

    protected EventCapture Events => TestScopedServices.GetCurrent().Events;

    protected FakeTimeProvider Clock => TestScopedServices.GetCurrent().Clock;

    protected IDbContextFactory<TrsDbContext> DbContextFactory => _fixture.Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    protected TestData TestData => _fixture.Services.GetRequiredService<TestData>();

    protected Task<T> WithDbContextAsync<T>(Func<TrsDbContext, Task<T>> action) =>
        DbContextFactory.WithDbContextAsync(action);

    protected Task WithDbContextAsync(Func<TrsDbContext, Task> action) =>
        DbContextFactory.WithDbContextAsync(action);

    protected Task WithServiceAsync<TService>(Func<TService, Task> action, params object[] arguments) where TService : notnull =>
        _fixture.WithServiceAsync(action, arguments);

    protected Task<TResult> WithServiceAsync<TService, TResult>(Func<TService, Task<TResult>> action, params object[] arguments) where TService : notnull =>
        _fixture.WithServiceAsync(action, arguments);
}

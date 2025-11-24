using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Tests.Services;

public abstract class ServiceTestBase
{
    private readonly ServiceFixture _fixture;

    protected ServiceTestBase(ServiceFixture fixture)
    {
        _fixture = fixture;

        TestScopedServices.Reset();
    }

    protected TestableClock Clock => TestScopedServices.GetCurrent().Clock;

    protected IDbContextFactory<TrsDbContext> DbContextFactory => _fixture.Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    protected EventCapture Events => _fixture.Services.GetRequiredService<EventCapture>();

    protected IServiceProvider Services => _fixture.Services;

    protected TestData TestData => Services.GetRequiredService<TestData>();

    protected Task<T> WithDbContextAsync<T>(Func<TrsDbContext, Task<T>> action) =>
        DbContextFactory.WithDbContextAsync(action);

    protected Task WithDbContextAsync(Func<TrsDbContext, Task> action) =>
        DbContextFactory.WithDbContextAsync(action);

    protected void WithService<TService>(Action<TService> action, params object[] arguments) where TService : notnull =>
        _fixture.WithService(action, arguments);

    protected TResult WithService<TService, TResult>(Func<TService, TResult> action, params object[] arguments) where TService : notnull =>
        _fixture.WithService(action, arguments);

    protected Task WithServiceAsync<TService>(Func<TService, Task> action, params object[] arguments) where TService : notnull =>
        _fixture.WithServiceAsync(action, arguments);

    protected Task<TResult> WithServiceAsync<TService, TResult>(Func<TService, Task<TResult>> action, params object[] arguments) where TService : notnull =>
        _fixture.WithServiceAsync(action, arguments);
}

using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection("NightEmailJobCollection")]
public abstract class NightlyEmailJobTestBase(CoreFixture fixture) : IAsyncLifetime
{
    public IDbContextFactory<TrsDbContext> DbContextFactory => fixture.DbContextFactory;

    public TestData TestData => fixture.TestData;

    public TestableClock Clock => fixture.Clock;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public async ValueTask InitializeAsync() => await fixture.DbHelper.ClearDataAsync();
}

namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection("NightEmailJobCollection")]
public abstract class NightlyEmailJobTestBase(NightlyEmailJobFixture fixture) : IAsyncLifetime
{
    public NightlyEmailJobFixture Fixture { get; } = fixture;

    public DbFixture DbFixture => Fixture.DbFixture;

    public TestData TestData => Fixture.TestData;

    public TestableClock Clock => Fixture.Clock;

    public Task DisposeAsync() => Task.CompletedTask;

    public Task InitializeAsync() => Fixture.DbFixture.DbHelper.ClearDataAsync();
}

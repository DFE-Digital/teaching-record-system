namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection("NightEmailJobCollection")]
public abstract class InductionCompletedEmailJobTestBase(NightlyEmailJobFixture fixture) : IAsyncLifetime
{
    public NightlyEmailJobFixture Fixture { get; } = fixture;

    public TestData TestData => Fixture.TestData;

    public TestableClock Clock => Fixture.Clock;

    public Task DisposeAsync() => Task.CompletedTask;

    public Task InitializeAsync() =>
        Fixture.DbFixture.WithDbContextAsync(dbContext => dbContext.Database.ExecuteSqlAsync($"delete from induction_completed_emails_jobs"));
}

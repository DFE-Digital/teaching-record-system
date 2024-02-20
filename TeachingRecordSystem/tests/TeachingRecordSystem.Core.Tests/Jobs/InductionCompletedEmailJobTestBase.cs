namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection("InductionCompletedEmailJob")]
public abstract class InductionCompletedEmailJobTestBase : IAsyncLifetime
{
    public InductionCompletedEmailJobTestBase(DbFixture dbFixture)
    {
        DbFixture = dbFixture;
    }

    public DbFixture DbFixture { get; }

    public Task DisposeAsync() => Task.CompletedTask;

    public Task InitializeAsync() =>
        DbFixture.WithDbContext(dbContext => dbContext.Database.ExecuteSqlAsync($"delete from induction_completed_emails_jobs"));
}

namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection("InternationalQtsAwardedEmailJob")]
public abstract class InternationalQtsAwardedEmailJobTestBase : IAsyncLifetime
{
    public InternationalQtsAwardedEmailJobTestBase(DbFixture dbFixture)
    {
        DbFixture = dbFixture;
    }

    public DbFixture DbFixture { get; }

    public Task DisposeAsync() => Task.CompletedTask;

    public Task InitializeAsync() =>
        DbFixture.WithDbContext(dbContext => dbContext.Database.ExecuteSqlAsync($"delete from international_qts_awarded_emails_jobs"));
}

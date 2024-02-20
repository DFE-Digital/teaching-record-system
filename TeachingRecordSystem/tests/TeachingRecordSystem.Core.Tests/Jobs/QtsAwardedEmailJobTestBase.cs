namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection("QtsAwardedEmailJob")]
public abstract class QtsAwardedEmailJobTestBase : IAsyncLifetime
{
    public QtsAwardedEmailJobTestBase(DbFixture dbFixture)
    {
        DbFixture = dbFixture;
    }

    public DbFixture DbFixture { get; }

    public Task DisposeAsync() => Task.CompletedTask;

    public Task InitializeAsync() =>
        DbFixture.WithDbContext(dbContext => dbContext.Database.ExecuteSqlAsync($"delete from qts_awarded_emails_jobs"));
}

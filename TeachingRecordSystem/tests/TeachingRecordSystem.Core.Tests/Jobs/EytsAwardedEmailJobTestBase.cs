namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection("EytsAwardedEmailJob")]
public abstract class EytsAwardedEmailJobTestBase : IAsyncLifetime
{
    public EytsAwardedEmailJobTestBase(DbFixture dbFixture)
    {
        DbFixture = dbFixture;
    }

    public DbFixture DbFixture { get; }

    public Task DisposeAsync() => Task.CompletedTask;

    public Task InitializeAsync() =>
        DbFixture.WithDbContext(dbContext => dbContext.Database.ExecuteSqlAsync($"delete from eyts_awarded_emails_jobs"));
}

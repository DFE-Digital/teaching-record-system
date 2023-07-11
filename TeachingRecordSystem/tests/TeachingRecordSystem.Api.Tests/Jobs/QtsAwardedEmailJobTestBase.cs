using Microsoft.EntityFrameworkCore;

namespace TeachingRecordSystem.Api.Tests.Jobs;

[Collection("QtsAwardedEmailJob")]
public abstract class QtsAwardedEmailJobTestBase : IAsyncLifetime
{
    public QtsAwardedEmailJobTestBase(DbFixture dbFixture)
    {
        DbFixture = dbFixture;
    }

    public DbFixture DbFixture { get; }

    public async Task InitializeAsync()
    {
        using var dbContext = DbFixture.GetDbContext();
        await dbContext.Database.ExecuteSqlAsync($"delete from qts_awarded_emails_jobs");
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

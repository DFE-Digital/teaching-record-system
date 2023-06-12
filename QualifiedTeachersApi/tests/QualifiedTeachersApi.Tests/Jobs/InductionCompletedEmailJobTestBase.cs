using Microsoft.EntityFrameworkCore;
using Xunit;

namespace QualifiedTeachersApi.Tests.Jobs;

[Collection("InductionCompletedEmailJob")]
public abstract class InductionCompletedEmailJobTestBase : IAsyncLifetime
{
    public InductionCompletedEmailJobTestBase(DbFixture dbFixture)
    {
        DbFixture = dbFixture;
    }

    public DbFixture DbFixture { get; }

    public async Task InitializeAsync()
    {
        using var dbContext = DbFixture.GetDbContext();
        await dbContext.Database.ExecuteSqlAsync($"delete from induction_completed_emails_jobs");
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

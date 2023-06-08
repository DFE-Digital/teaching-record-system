using Microsoft.EntityFrameworkCore;
using Xunit;

namespace QualifiedTeachersApi.Tests.Jobs;

[Collection("InternationalQtsAwardedEmailJob")]
public abstract class InternationalQtsAwardedEmailJobTestBase : IAsyncLifetime
{
    public InternationalQtsAwardedEmailJobTestBase(DbFixture dbFixture)
    {
        DbFixture = dbFixture;
    }

    public DbFixture DbFixture { get; }

    public async Task InitializeAsync()
    {
        using var dbContext = DbFixture.GetDbContext();
        await dbContext.Database.ExecuteSqlAsync($"delete from international_qts_awarded_emails_jobs");
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

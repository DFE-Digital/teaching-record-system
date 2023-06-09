using Microsoft.EntityFrameworkCore;
using Xunit;

namespace QualifiedTeachersApi.Tests.Jobs;

[Collection("EytsAwardedEmailJob")]
public abstract class EytsAwardedEmailJobTestBase : IAsyncLifetime
{
    public EytsAwardedEmailJobTestBase(DbFixture dbFixture)
    {
        DbFixture = dbFixture;
    }

    public DbFixture DbFixture { get; }

    public async Task InitializeAsync()
    {
        using var dbContext = DbFixture.GetDbContext();
        await dbContext.Database.ExecuteSqlAsync($"delete from eyts_awarded_emails_jobs");
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

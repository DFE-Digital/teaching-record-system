using Microsoft.EntityFrameworkCore;
using Moq;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.Jobs.Scheduling;
using QualifiedTeachersApi.Services.GetAnIdentityApi;
using QualifiedTeachersApi.Services.Notify;
using Xunit;

namespace QualifiedTeachersApi.Tests.Jobs;

public class JobFixture : IAsyncLifetime
{
    public JobFixture(DbFixture dbFixture)
    {
        DbFixture = dbFixture;
    }

    public DbFixture DbFixture { get; }

    public Mock<IDataverseAdapter> DataverseAdapter { get; } = new Mock<IDataverseAdapter>();

    public Mock<IBackgroundJobScheduler> BackgroundJobScheduler { get; } = new Mock<IBackgroundJobScheduler>();

    public Mock<INotificationSender> NotificationSender { get; } = new Mock<INotificationSender>();

    public Mock<IGetAnIdentityApiClient> GetAnIdentityApiClient { get; } = new Mock<IGetAnIdentityApiClient>();

    public TestableClock Clock { get; } = new TestableClock();

    public async Task InitializeAsync()
    {
        await DbFixture.InitializeAsync();
    }

    public void ResetMocks()
    {
        DataverseAdapter.Reset();
        BackgroundJobScheduler.Reset();
        NotificationSender.Reset();
        GetAnIdentityApiClient.Reset();
    }

    public async Task ClearData()
    {
        using var dbContext = DbFixture.GetDbContext();
        await dbContext.Database.ExecuteSqlAsync($"delete from qts_awarded_emails_jobs");
    }

    public async Task DisposeAsync()
    {
        await ((IAsyncLifetime)DbFixture).DisposeAsync();
    }
}

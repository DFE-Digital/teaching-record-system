using System.Diagnostics;
using Hangfire;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BatchSendInductionCompletedEmailsJobTests(NightlyEmailJobFixture dbFixture) : NightlyEmailJobTestBase(dbFixture)
{
    [Fact]
    public async Task Execute_EnqueuesEmailForInductionCompletees()
    {
        // Arrange
        var initialLastAwardedToUtc = Clock.Today.AddDays(-5).ToDateTime();
        var backgroundJobScheduler = new Mock<IBackgroundJobScheduler>();

        var jobOptions = Options.Create(
            new BatchSendInductionCompletedEmailsJobOptions()
            {
                EmailDelayDays = 3,
                InitialLastPassedEndUtc = initialLastAwardedToUtc,
                JobSchedule = Cron.Never()
            });

        var inductionStartDate = new DateOnly(2020, 9, 1);
        var inductionCompletedDate = new DateOnly(2021, 10, 10);

        var inductionCompletee1 = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts()
            .WithEmail(TestData.GenerateUniqueEmail()));

        await using var dbContext = await Fixture.DbFixture.GetDbContextFactory().CreateDbContextAsync();

        await Fixture.DbFixture.WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(inductionCompletee1.Person);

            inductionCompletee1.Person.SetInductionStatus(
                InductionStatus.Passed,
                inductionStartDate,
                inductionCompletedDate,
                exemptionReasonIds: [],
                changeReason: null,
                changeReasonDetail: null,
                evidenceFile: null,
                updatedBy: SystemUser.SystemUserId,
                now: Clock.UtcNow,
                out var @event);

            Debug.Assert(@event is not null);
            dbContext.AddEventWithoutBroadcast(@event);

            await dbContext.SaveChangesAsync();
        });

        Clock.Advance(TimeSpan.FromDays(jobOptions.Value.EmailDelayDays + 2));

        var job = new BatchSendInductionCompletedEmailsJob(
            jobOptions,
            dbContext,
            backgroundJobScheduler.Object,
            Clock);

        // Act
        await job.ExecuteAsync(CancellationToken.None);

        // Assert
        var jobItem = await dbContext.InductionCompletedEmailsJobItems.SingleOrDefaultAsync(
            i => i.PersonId == inductionCompletee1.PersonId);
        Assert.NotNull(jobItem);
        Assert.Equal(inductionCompletee1.Trn, jobItem.Trn);
        Assert.Equal(inductionCompletee1.Email, jobItem.EmailAddress);
        Assert.Equal(inductionCompletee1.FirstName, jobItem.Personalization["first name"]);
        Assert.Equal(inductionCompletee1.LastName, jobItem.Personalization["last name"]);

        backgroundJobScheduler
            .Verify(
                s => s.EnqueueAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<SendInductionCompletedEmailJob, Task>>>()),
                Times.Once);
    }
}

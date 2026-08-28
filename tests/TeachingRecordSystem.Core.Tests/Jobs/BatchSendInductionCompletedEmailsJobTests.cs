using System.Diagnostics;
using Hangfire;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Inductions;
using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BatchSendInductionCompletedEmailsJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_EnqueuesEmailForInductionCompletees()
    {
        // Arrange
        var initialLastAwardedToUtc = TimeProvider.Today.AddDays(-5).ToDateTime();
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
            .WithQts()
            .WithEmailAddress(TestData.GenerateUniqueEmail()));

        var passed = await WithServiceAsync<InductionService, bool>(inductionService =>
            inductionService.SetInductionStatusAsync(
                new SetInductionStatusOptions
                {
                    PersonId = inductionCompletee1.PersonId,
                    Status = InductionStatus.Passed,
                    StartDate = inductionStartDate,
                    CompletedDate = inductionCompletedDate,
                    ExemptionReasonIds = []
                },
                new ProcessContext(
                    ProcessType.PersonInductionUpdating,
                    TimeProvider.UtcNow,
                    SystemUser.SystemUserId)));

        Debug.Assert(passed);

        TimeProvider.Advance(TimeSpan.FromDays(jobOptions.Value.EmailDelayDays + 2));

        // Act
        await WithServiceAsync<BatchSendInductionCompletedEmailsJob>(
            job => job.ExecuteAsync(CancellationToken.None),
            jobOptions,
            backgroundJobScheduler.Object);

        // Assert
        var jobItem = await WithDbContextAsync(dbContext => dbContext.InductionCompletedEmailsJobItems.SingleOrDefaultAsync(
            i => i.PersonId == inductionCompletee1.PersonId));
        Assert.NotNull(jobItem);
        Assert.Equal(inductionCompletee1.Trn, jobItem.Trn);
        Assert.Equal(inductionCompletee1.EmailAddress, jobItem.EmailAddress);
        Assert.Equal(inductionCompletee1.FirstName, jobItem.Personalization["first name"]);
        Assert.Equal(inductionCompletee1.LastName, jobItem.Personalization["last name"]);

        backgroundJobScheduler
            .Verify(
                s => s.EnqueueAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<SendInductionCompletedEmailJob, Task>>>()),
                Times.Once);
    }
}

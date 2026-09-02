using System.Diagnostics;
using Hangfire;
using Microsoft.Extensions.Options;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Inductions;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;
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

    [Fact]
    public async Task Execute_InductionPassedOnAnotherProcessType_EnqueuesEmail()
    {
        // Arrange
        // PersonInductionUpdatedEvent isn't published only by PersonInductionUpdating - a route change moves the
        // person's induction too - so the job has to key off the event, not the process it's attached to.
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

        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithEmailAddress(TestData.GenerateUniqueEmail()));

        // Set the status without going through a process, so the only event the job can find is the one below.
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person);

            person.UnsafeSetInductionStatus(
                InductionStatus.Passed,
                InductionStatus.Passed,
                inductionStartDate,
                inductionCompletedDate,
                exemptionReasonIds: []);

            await dbContext.SaveChangesAsync();
        });

        await AddInductionUpdatedEventOnProcessAsync(
            person.PersonId,
            ProcessType.RouteToProfessionalStatusUpdating,
            CreateInduction(InductionStatus.Passed, inductionStartDate, inductionCompletedDate),
            CreateInduction(InductionStatus.InProgress, inductionStartDate, null));

        TimeProvider.Advance(TimeSpan.FromDays(jobOptions.Value.EmailDelayDays + 2));

        // Act
        await WithServiceAsync<BatchSendInductionCompletedEmailsJob>(
            job => job.ExecuteAsync(CancellationToken.None),
            jobOptions,
            backgroundJobScheduler.Object);

        // Assert
        var jobItem = await WithDbContextAsync(dbContext => dbContext.InductionCompletedEmailsJobItems.SingleOrDefaultAsync(
            i => i.PersonId == person.PersonId));
        Assert.NotNull(jobItem);
        Assert.Equal(person.Trn, jobItem.Trn);

        backgroundJobScheduler
            .Verify(
                s => s.EnqueueAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<SendInductionCompletedEmailJob, Task>>>()),
                Times.Once);
    }

    private static EventModels.Induction CreateInduction(
        InductionStatus status,
        DateOnly? startDate,
        DateOnly? completedDate) => new()
        {
            Status = status,
            StatusWithoutExemption = status,
            StartDate = startDate,
            CompletedDate = completedDate,
            ExemptionReasonIds = [],
            CpdCpdModifiedOn = Option.None<DateTime>(),
            InductionExemptWithoutReason = false
        };

    private Task AddInductionUpdatedEventOnProcessAsync(
        Guid personId,
        ProcessType processType,
        EventModels.Induction induction,
        EventModels.Induction oldInduction) =>
        WithDbContextAsync(async dbContext =>
        {
            var processId = Guid.NewGuid();

            var updatedEvent = new PersonInductionUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = personId,
                Induction = induction,
                OldInduction = oldInduction,
                Changes = PersonInductionUpdatedEvent.GetChanges(induction, oldInduction)
            };

            dbContext.Processes.Add(new Process
            {
                ProcessId = processId,
                ProcessType = processType,
                CreatedOn = TimeProvider.UtcNow,
                UpdatedOn = TimeProvider.UtcNow,
                UserId = SystemUser.SystemUserId,
                DqtUserId = null,
                DqtUserName = null,
                PersonIds = [personId],
                OneLoginUserSubjects = [],
                SupportTaskReferences = [],
                ChangeReason = null
            });

            dbContext.Set<ProcessEvent>().Add(new ProcessEvent
            {
                ProcessEventId = updatedEvent.EventId,
                ProcessId = processId,
                EventName = nameof(PersonInductionUpdatedEvent),
                Payload = updatedEvent,
                PersonIds = [personId],
                OneLoginUserSubjects = [],
                SupportTaskReferences = [],
                CreatedOn = TimeProvider.UtcNow
            });

            await dbContext.SaveChangesAsync();
        });
}

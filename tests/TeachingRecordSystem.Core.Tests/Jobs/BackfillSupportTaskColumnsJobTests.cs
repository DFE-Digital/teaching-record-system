using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillSupportTaskColumnsJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task ExecuteAsync_PersonSubjectTask_SetsSubjectNameToPersonNameAtTaskCreation()
    {
        // Arrange
        var originalFirstName = "Alice";
        var originalLastName = "Original";
        var person = await TestData.CreatePersonAsync(p => p.WithFirstName(originalFirstName).WithLastName(originalLastName));

        var createdOn = TimeProvider.UtcNow;
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            person.PersonId,
            t => t.WithCreatedOn(createdOn));

        // Simulate a pre-migration task by clearing the newly-added subject columns.
        await ClearSubjectColumnsAsync(supportTask);

        // Rename the person after the task was created, recording the change as an event.
        var changedLastName = "Changed";
        await WithDbContextAsync(async dbContext =>
        {
            var dbPerson = await dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId);
            var oldPersonAttributes = EventModels.PersonDetails.FromModel(dbPerson);
            dbPerson.LastName = changedLastName;
            var newPersonAttributes = EventModels.PersonDetails.FromModel(dbPerson);

            dbContext.AddEventWithoutBroadcast(new LegacyEvents.PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = createdOn.AddDays(1),
                RaisedBy = SystemUser.SystemUserId,
                PersonId = person.PersonId,
                PersonAttributes = newPersonAttributes,
                OldPersonAttributes = oldPersonAttributes,
                NameChangeReason = null,
                NameChangeEvidenceFile = null,
                DetailsChangeReason = null,
                DetailsChangeReasonDetail = null,
                DetailsChangeEvidenceFile = null,
                Changes = LegacyEvents.PersonDetailsUpdatedEventChanges.LastName
            });
            await dbContext.SaveChangesAsync();
        });

        // Act
        await WithDbContextAsync(dbContext =>
            new BackfillSupportTaskColumnsJob(dbContext).ExecuteAsync(dryRun: false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var dbSupportTask = await dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            var expectedName = string.JoinNonEmpty(' ', originalFirstName, person.MiddleName, originalLastName);
            Assert.Equal(expectedName, dbSupportTask.SubjectName);
            Assert.Null(dbSupportTask.SubjectEmailAddress);
        });
    }

    [Fact]
    public async Task ExecuteAsync_OneLoginRecordMatchingTask_SetsSubjectNameFromVerifiedNames()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject,
            t => t.WithVerifiedNames(["Bob", "Smith"]));

        await ClearSubjectColumnsAsync(supportTask);

        // Act
        await WithDbContextAsync(dbContext =>
            new BackfillSupportTaskColumnsJob(dbContext).ExecuteAsync(dryRun: false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var dbSupportTask = await dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal("Bob Smith", dbSupportTask.SubjectName);
            Assert.Null(dbSupportTask.SubjectEmailAddress);
        });
    }

    [Fact]
    public async Task ExecuteAsync_TeacherPensionsTask_SetsSubjectNameOnlyFromTrnRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            person.PersonId,
            user.UserId,
            t => t.WithFirstName("Carol").WithMiddleName(null).WithLastName("Jones").WithEmailAddress(TestData.GenerateUniqueEmail()));

        await ClearSubjectColumnsAsync(supportTask);

        // Act
        await WithDbContextAsync(dbContext =>
            new BackfillSupportTaskColumnsJob(dbContext).ExecuteAsync(dryRun: false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var dbSupportTask = await dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal("Carol Jones", dbSupportTask.SubjectName);
            Assert.Null(dbSupportTask.SubjectEmailAddress);
        });
    }

    [Fact]
    public async Task ExecuteAsync_ClosedTaskWithLegacyClosingEvent_BackfillsCompletionOutcomeAndEventOutcomeLabel()
    {
        // Arrange
        var closedByUser = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();

        var createdOn = TimeProvider.UtcNow;
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            person.PersonId,
            t => t.WithStatus(SupportTaskStatus.Closed).WithCreatedOn(createdOn));

        var outcome = SupportRequestOutcome.Rejected;
        var closedOn = createdOn.AddDays(2);
        var closingEventId = Guid.NewGuid();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(supportTask);

            // A closed task carries its outcome on its data.
            var data = supportTask.GetData<ChangeNameRequestData>() with { ChangeRequestOutcome = outcome };
            supportTask.Data = data;

            // Simulate a pre-migration task.
            dbContext.Entry(supportTask).Property(t => t.OutcomeLabel).CurrentValue = null;
            supportTask.CompletedOn = null;
            supportTask.CompletedByUserId = null;

            var oldSupportTask = EventModels.SupportTask.FromModel(supportTask) with { Status = SupportTaskStatus.Open, OutcomeLabel = null };
            var newSupportTask = EventModels.SupportTask.FromModel(supportTask) with { Status = SupportTaskStatus.Closed, OutcomeLabel = null };

            dbContext.AddEventWithoutBroadcast(new LegacyEvents.ChangeNameRequestSupportTaskRejectedEvent
            {
                EventId = closingEventId,
                CreatedUtc = closedOn,
                RaisedBy = closedByUser.UserId,
                PersonId = person.PersonId,
                RequestData = EventModels.ChangeNameRequestData.FromModel(data),
                RejectionReason = "Request and proof don't match",
                SupportTask = newSupportTask,
                OldSupportTask = oldSupportTask
            });

            await dbContext.SaveChangesAsync();
        });

        // Act
        await WithDbContextAsync(dbContext =>
            new BackfillSupportTaskColumnsJob(dbContext).ExecuteAsync(dryRun: false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var dbSupportTask = await dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(closedOn, dbSupportTask.CompletedOn);
            Assert.Equal(closedByUser.UserId, dbSupportTask.CompletedByUserId);
            Assert.Equal(nameof(SupportRequestOutcome.Rejected), dbSupportTask.OutcomeLabel);

            var closingEvent = await dbContext.Events.SingleAsync(e => e.EventId == closingEventId);
            var closingEventPayload = (LegacyEvents.ChangeNameRequestSupportTaskRejectedEvent)closingEvent.ToEventBase();
            Assert.Equal(nameof(SupportRequestOutcome.Rejected), closingEventPayload.SupportTask.OutcomeLabel);
        });
    }

    private Task ClearSubjectColumnsAsync(SupportTask supportTask) =>
        WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(supportTask);
            dbContext.Entry(supportTask).Property(t => t.SubjectName).CurrentValue = null;
            dbContext.Entry(supportTask).Property(t => t.SubjectEmailAddress).CurrentValue = null;
            await dbContext.SaveChangesAsync();
        });
}

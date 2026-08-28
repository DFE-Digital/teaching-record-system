using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Jobs;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillPersonInductionProcessesJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_LegacyUpdatedEvent_CreatesProcessAndProcessEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.PersonInductionUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = user.UserId,
            PersonId = person.PersonId,
            Induction = CreateInduction(InductionStatus.Passed, new DateOnly(2024, 1, 1), new DateOnly(2025, 1, 1)),
            OldInduction = CreateInduction(InductionStatus.InProgress, new DateOnly(2024, 1, 1), null),
            ChangeReason = "Some reason",
            ChangeReasonDetail = "Some detail",
            EvidenceFile = null,
            AdditionalInformation = "Some additional information",
            Changes = LegacyEvents.PersonInductionUpdatedEventChanges.InductionStatus |
                LegacyEvents.PersonInductionUpdatedEventChanges.InductionCompletedDate
        });

        // Act
        await WithServiceAsync<BackfillPersonInductionProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(PersonInductionUpdatedEvent), processEvent.EventName);
            Assert.Equal(person.PersonId, Assert.Single(processEvent.PersonIds));

            var updatedEvent = Assert.IsType<PersonInductionUpdatedEvent>(processEvent.Payload);
            Assert.Equal(person.PersonId, updatedEvent.PersonId);
            Assert.Equal(InductionStatus.Passed, updatedEvent.Induction.Status);
            Assert.Equal(new DateOnly(2025, 1, 1), updatedEvent.Induction.CompletedDate);
            Assert.Equal(InductionStatus.InProgress, updatedEvent.OldInduction.Status);
            Assert.Null(updatedEvent.OldInduction.CompletedDate);
            Assert.Equal(
                PersonInductionUpdatedEventChanges.InductionStatus | PersonInductionUpdatedEventChanges.InductionCompletedDate,
                updatedEvent.Changes);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.PersonInductionUpdating, process.ProcessType);
            Assert.Equal(user.UserId, process.UserId);
            Assert.Null(process.DqtUserId);
            Assert.Equal(legacyEvent.CreatedUtc, process.CreatedOn);
            Assert.Equal(person.PersonId, Assert.Single(process.PersonIds));

            // The reason, its detail and the additional information move off the event and onto the process.
            var changeReason = Assert.IsType<ChangeReasonWithDetailsAndEvidence>(process.ChangeReason);
            Assert.Equal("Some reason", changeReason.Reason);
            Assert.Equal("Some detail", changeReason.Details);
            Assert.Equal("Some additional information", changeReason.AdditionalInformation);
            Assert.Null(changeReason.EvidenceFile);
        });
    }

    [Fact]
    public async Task Execute_LegacyUpdatedEventWithEvidenceFile_PutsTheEvidenceFileOnTheProcess()
    {
        // Arrange
        var fileId = Guid.NewGuid();

        var legacyEvent = await AddUpdatedEventAsync(evidenceFile: new EventModels.File { FileId = fileId, Name = "evidence.pdf" });

        // Act
        await WithServiceAsync<BackfillPersonInductionProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);

            var changeReason = Assert.IsType<ChangeReasonWithDetailsAndEvidence>(process.ChangeReason);
            Assert.Equal(fileId, changeReason.EvidenceFile?.FileId);
            Assert.Equal("evidence.pdf", changeReason.EvidenceFile?.Name);
        });
    }

    [Fact]
    public async Task Execute_LegacyUpdatedEventWithNoReason_LeavesTheProcessWithoutAChangeReason()
    {
        // Arrange
        var legacyEvent = await AddUpdatedEventAsync(changeReason: null, additionalInformation: null);

        // Act
        await WithServiceAsync<BackfillPersonInductionProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);

            Assert.Null(process.ChangeReason);
        });
    }

    [Fact]
    public async Task Execute_LegacyEventRaisedByDqtUser_PutsTheDqtUserOnTheProcess()
    {
        // Arrange
        var dqtUserId = Guid.NewGuid();

        var legacyEvent = await AddUpdatedEventAsync(
            raisedBy: EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId, "DQT User"));

        // Act
        await WithServiceAsync<BackfillPersonInductionProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);

            Assert.Null(process.UserId);
            Assert.Equal(dqtUserId, process.DqtUserId);
            Assert.Equal("DQT User", process.DqtUserName);
        });
    }

    [Fact]
    public async Task Execute_MoreEventsThanFitInOneBatch_MigratesThemAll()
    {
        // Arrange
        // The job's batch size is 5000; go a little over it so more than one batch is needed.
        const int eventCount = 5005;

        var person = await TestData.CreatePersonAsync();

        var legacyEvents = Enumerable.Range(0, eventCount)
            .Select(i => CreateLegacyEvent(
                person.PersonId,
                // Distinct timestamps so the (created, event_id) paging has a stable order to walk.
                createdUtc: TimeProvider.UtcNow.AddSeconds(i)))
            .ToArray();

        await AddLegacyEventsAsync(legacyEvents);

        // Act
        await WithServiceAsync<BackfillPersonInductionProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var eventIds = legacyEvents.Select(e => e.EventId).ToArray();
            var migratedCount = await dbContext.ProcessEvents.CountAsync(pe => eventIds.Contains(pe.ProcessEventId));
            Assert.Equal(eventCount, migratedCount);
        });
    }

    [Fact]
    public async Task Execute_EventsWithTheSameCreatedTimestamp_MigratesThemAll()
    {
        // Arrange
        // The cursor is (created, event_id), so events sharing a timestamp shouldn't be skipped or repeated.
        const int eventCount = 50;

        var person = await TestData.CreatePersonAsync();
        var created = TimeProvider.UtcNow;

        var legacyEvents = Enumerable.Range(0, eventCount)
            .Select(_ => CreateLegacyEvent(person.PersonId, createdUtc: created))
            .ToArray();

        await AddLegacyEventsAsync(legacyEvents);

        // Act
        await WithServiceAsync<BackfillPersonInductionProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var eventIds = legacyEvents.Select(e => e.EventId).ToArray();
            var migratedCount = await dbContext.ProcessEvents.CountAsync(pe => eventIds.Contains(pe.ProcessEventId));
            Assert.Equal(eventCount, migratedCount);
        });
    }

    [Fact]
    public async Task Execute_RunTwice_DoesNotBackfillTwice()
    {
        // Arrange
        var legacyEvent = await AddUpdatedEventAsync();

        await WithServiceAsync<BackfillPersonInductionProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Act
        await WithServiceAsync<BackfillPersonInductionProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvents = await dbContext.ProcessEvents.Where(pe => pe.ProcessEventId == legacyEvent.EventId).ToListAsync();
            Assert.Single(processEvents);

            var processes = await dbContext.Processes
                .Where(p => p.ProcessType == ProcessType.PersonInductionUpdating)
                .ToListAsync();
            Assert.Single(processes);
        });
    }

    [Fact]
    public async Task Execute_DryRun_DoesNotCommitChanges()
    {
        // Arrange
        var legacyEvent = await AddUpdatedEventAsync();

        // Act
        await WithServiceAsync<BackfillPersonInductionProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    [Fact]
    public async Task Execute_LegacyEventOfAnotherType_IsNotBackfilled()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.DqtQtsRegistrationCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            QtsRegistration = new EventModels.DqtQtsRegistration { TeacherStatusName = "Trainee teacher" }
        });

        // Act
        await WithServiceAsync<BackfillPersonInductionProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    private static EventModels.Induction CreateInduction(
        InductionStatus status = InductionStatus.None,
        DateOnly? startDate = null,
        DateOnly? completedDate = null) => new()
        {
            Status = status,
            StatusWithoutExemption = status,
            StartDate = startDate,
            CompletedDate = completedDate,
            ExemptionReasonIds = [],
            CpdCpdModifiedOn = Option.None<DateTime>(),
            InductionExemptWithoutReason = false
        };

    private LegacyEvents.PersonInductionUpdatedEvent CreateLegacyEvent(
        Guid personId,
        DateTime? createdUtc = null,
        EventModels.RaisedByUserInfo? raisedBy = null,
        string? changeReason = "Some reason",
        string? additionalInformation = null,
        EventModels.File? evidenceFile = null) =>
        new()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = createdUtc ?? TimeProvider.UtcNow,
            RaisedBy = raisedBy ?? SystemUser.SystemUserId,
            PersonId = personId,
            Induction = CreateInduction(InductionStatus.RequiredToComplete),
            OldInduction = CreateInduction(),
            ChangeReason = changeReason,
            ChangeReasonDetail = null,
            EvidenceFile = evidenceFile,
            AdditionalInformation = additionalInformation,
            Changes = LegacyEvents.PersonInductionUpdatedEventChanges.InductionStatus
        };

    private async Task<LegacyEvents.PersonInductionUpdatedEvent> AddUpdatedEventAsync(
        EventModels.RaisedByUserInfo? raisedBy = null,
        string? changeReason = "Some reason",
        string? additionalInformation = null,
        EventModels.File? evidenceFile = null)
    {
        var person = await TestData.CreatePersonAsync();

        return await AddLegacyEventAsync(
            CreateLegacyEvent(person.PersonId, raisedBy: raisedBy, changeReason: changeReason,
                additionalInformation: additionalInformation, evidenceFile: evidenceFile));
    }

    private async Task<TEvent> AddLegacyEventAsync<TEvent>(TEvent legacyEvent) where TEvent : LegacyEvents.EventBase
    {
        await AddLegacyEventsAsync([legacyEvent]);
        return legacyEvent;
    }

    private Task AddLegacyEventsAsync(IEnumerable<LegacyEvents.EventBase> legacyEvents) =>
        WithDbContextAsync(async dbContext =>
        {
            foreach (var legacyEvent in legacyEvents)
            {
                dbContext.AddEventWithoutBroadcast(legacyEvent);
            }

            await dbContext.SaveChangesAsync();
        });
}

using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillInductionMigrationProcessesJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_LegacyMigratedEvent_CreatesProcessAndProcessEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var exemptionReasonId = InductionExemptionReason.QualifiedThroughEeaMutualRecognitionRouteId;
        var dqtInduction = CreateDqtInduction(inductionExemptionReason: "Qualified through EEA mutual recognition route");

        var legacyEvent = await AddLegacyEventAsync(CreateLegacyEvent(
            person.PersonId,
            inductionStatus: InductionStatus.Exempt,
            inductionExemptionReasonId: exemptionReasonId,
            inductionStartDate: new DateOnly(2020, 9, 1),
            inductionCompletedDate: new DateOnly(2021, 9, 1),
            dqtInduction: dqtInduction,
            dqtInductionStatus: "Exempt"));

        // Act
        await WithServiceAsync<BackfillInductionMigrationProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(InductionMigratedEvent), processEvent.EventName);
            Assert.Equal(person.PersonId, Assert.Single(processEvent.PersonIds));
            Assert.Equal(legacyEvent.CreatedUtc, processEvent.CreatedOn);

            var migratedEvent = Assert.IsType<InductionMigratedEvent>(processEvent.Payload);
            Assert.Equal(person.PersonId, migratedEvent.PersonId);
            Assert.Equal(InductionStatus.Exempt, migratedEvent.InductionStatus);
            Assert.Equal(exemptionReasonId, migratedEvent.InductionExemptionReasonId);
            Assert.Equal(new DateOnly(2020, 9, 1), migratedEvent.InductionStartDate);
            Assert.Equal(new DateOnly(2021, 9, 1), migratedEvent.InductionCompletedDate);
            Assert.Equal("Exempt", migratedEvent.DqtInductionStatus);
            Assert.Equal(dqtInduction.InductionId, migratedEvent.DqtInduction?.InductionId);
            Assert.Equal(dqtInduction.InductionExemptionReason, migratedEvent.DqtInduction?.InductionExemptionReason);
            Assert.Equal(dqtInduction.StartDate, migratedEvent.DqtInduction?.StartDate);
            Assert.Equal(dqtInduction.CompletionDate, migratedEvent.DqtInduction?.CompletionDate);
            Assert.Equal(dqtInduction.InductionStatus, migratedEvent.DqtInduction?.InductionStatus);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.InductionMigratingFromDqt, process.ProcessType);
            Assert.Equal(SystemUser.SystemUserId, process.UserId);
            Assert.Null(process.DqtUserId);
            Assert.Equal(legacyEvent.CreatedUtc, process.CreatedOn);
            Assert.Equal(legacyEvent.CreatedUtc, process.UpdatedOn);
            Assert.Equal(person.PersonId, Assert.Single(process.PersonIds));

            // The migration never recorded a reason.
            Assert.Null(process.ChangeReason);
        });
    }

    [Fact]
    public async Task Execute_LegacyMigratedEventWithNothingPopulated_CreatesProcessAndProcessEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyEventAsync(CreateLegacyEvent(person.PersonId, dqtInduction: null));

        // Act
        await WithServiceAsync<BackfillInductionMigrationProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleAsync(pe => pe.ProcessEventId == legacyEvent.EventId);

            var migratedEvent = Assert.IsType<InductionMigratedEvent>(processEvent.Payload);
            Assert.Null(migratedEvent.DqtInduction);
            Assert.Null(migratedEvent.InductionExemptionReasonId);
            Assert.Null(migratedEvent.InductionStartDate);
            Assert.Null(migratedEvent.InductionCompletedDate);
            Assert.Equal(InductionStatus.None, migratedEvent.InductionStatus);
        });
    }

    [Fact]
    public async Task Execute_LegacyEventRaisedByDqtUser_PutsTheDqtUserOnTheProcess()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var dqtUserId = Guid.NewGuid();

        var legacyEvent = await AddLegacyEventAsync(CreateLegacyEvent(
            person.PersonId,
            raisedBy: EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId, "DQT User")));

        // Act
        await WithServiceAsync<BackfillInductionMigrationProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

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
        await WithServiceAsync<BackfillInductionMigrationProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

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
        await WithServiceAsync<BackfillInductionMigrationProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

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
        var person = await TestData.CreatePersonAsync();
        var legacyEvent = await AddLegacyEventAsync(CreateLegacyEvent(person.PersonId));

        await WithServiceAsync<BackfillInductionMigrationProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Act
        await WithServiceAsync<BackfillInductionMigrationProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvents = await dbContext.ProcessEvents.Where(pe => pe.ProcessEventId == legacyEvent.EventId).ToListAsync();
            Assert.Single(processEvents);

            var processes = await dbContext.Processes
                .Where(p => p.ProcessType == ProcessType.InductionMigratingFromDqt && p.PersonIds.Contains(person.PersonId))
                .ToListAsync();
            Assert.Single(processes);
        });
    }

    [Fact]
    public async Task Execute_LegacyEventOfAnotherType_IsNotBackfilled()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.DqtInductionCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            Induction = CreateDqtInduction()
        });

        // Act
        await WithServiceAsync<BackfillInductionMigrationProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    private static EventModels.DqtInduction CreateDqtInduction(string? inductionExemptionReason = null) => new()
    {
        InductionId = Guid.NewGuid(),
        StartDate = Option.Some<DateOnly?>(new DateOnly(2020, 9, 1)),
        CompletionDate = Option.None<DateOnly?>(),
        InductionStatus = Option.Some<string?>("InProgress"),
        InductionExemptionReason = inductionExemptionReason is not null
            ? Option.Some<string?>(inductionExemptionReason)
            : Option.None<string?>()
    };

    private LegacyEvents.InductionMigratedEvent CreateLegacyEvent(
        Guid personId,
        DateTime? createdUtc = null,
        EventModels.RaisedByUserInfo? raisedBy = null,
        InductionStatus inductionStatus = InductionStatus.None,
        Guid? inductionExemptionReasonId = null,
        DateOnly? inductionStartDate = null,
        DateOnly? inductionCompletedDate = null,
        EventModels.DqtInduction? dqtInduction = null,
        string dqtInductionStatus = "In progress") =>
        new()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = createdUtc ?? TimeProvider.UtcNow,
            RaisedBy = raisedBy ?? SystemUser.SystemUserId,
            PersonId = personId,
            InductionStatus = inductionStatus,
            InductionExemptionReasonId = inductionExemptionReasonId,
            InductionStartDate = inductionStartDate,
            InductionCompletedDate = inductionCompletedDate,
            DqtInduction = dqtInduction,
            DqtInductionStatus = dqtInductionStatus
        };

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

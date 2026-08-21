using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillDqtInductionAndIttProcessesJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_LegacyInductionCreatedEvent_CreatesProcessAndProcessEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var induction = CreateInduction();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.DqtInductionCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            Induction = induction
        });

        // Act
        await WithServiceAsync<BackfillDqtInductionAndIttProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(DqtInductionCreatedEvent), processEvent.EventName);
            Assert.Equal(person.PersonId, Assert.Single(processEvent.PersonIds));

            var createdEvent = Assert.IsType<DqtInductionCreatedEvent>(processEvent.Payload);
            Assert.Equal(induction.InductionId, createdEvent.Induction.InductionId);
            Assert.Equal(induction.StartDate, createdEvent.Induction.StartDate);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.InductionCreatingInDqt, process.ProcessType);
            Assert.Equal(SystemUser.SystemUserId, process.UserId);
            Assert.Equal(legacyEvent.CreatedUtc, process.CreatedOn);
            Assert.Equal(person.PersonId, Assert.Single(process.PersonIds));
        });
    }

    [Fact]
    public async Task Execute_LegacyInductionUpdatedEvent_CreatesProcessAndProcessEventWithChangesAndOldInduction()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var induction = CreateInduction();
        var oldInduction = induction with { StartDate = Option.Some<DateOnly?>(new DateOnly(2019, 1, 1)) };

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.DqtInductionUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            Induction = induction,
            OldInduction = oldInduction,
            Changes = LegacyEvents.DqtInductionUpdatedEventChanges.StartDate | LegacyEvents.DqtInductionUpdatedEventChanges.Status
        });

        // Act
        await WithServiceAsync<BackfillDqtInductionAndIttProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);

            var updatedEvent = Assert.IsType<DqtInductionUpdatedEvent>(processEvent.Payload);
            Assert.Equal(
                DqtInductionUpdatedEventChanges.StartDate | DqtInductionUpdatedEventChanges.Status,
                updatedEvent.Changes);
            Assert.Equal(oldInduction.StartDate, updatedEvent.OldInduction.StartDate);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.InductionUpdatingInDqt, process.ProcessType);
        });
    }

    [Fact]
    public async Task Execute_LegacyInductionImportedEvent_CreatesProcessAndProcessEventWithDqtState()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.DqtInductionImportedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            Induction = CreateInduction(),
            DqtState = 1
        });

        // Act
        await WithServiceAsync<BackfillDqtInductionAndIttProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);

            var importedEvent = Assert.IsType<DqtInductionImportedEvent>(processEvent.Payload);
            Assert.Equal(1, importedEvent.DqtState);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.InductionImportingIntoDqt, process.ProcessType);
        });
    }

    [Fact]
    public async Task Execute_LegacyInductionDeactivatedEvent_CreatesProcessAndProcessEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.DqtInductionDeactivatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            Induction = CreateInduction()
        });

        // Act
        await WithServiceAsync<BackfillDqtInductionAndIttProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(DqtInductionDeactivatedEvent), processEvent.EventName);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.InductionDeactivatingInDqt, process.ProcessType);
        });
    }

    [Fact]
    public async Task Execute_LegacyInductionReactivatedEvent_CreatesProcessAndProcessEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.DqtInductionReactivatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            Induction = CreateInduction()
        });

        // Act
        await WithServiceAsync<BackfillDqtInductionAndIttProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(DqtInductionReactivatedEvent), processEvent.EventName);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.InductionReactivatingInDqt, process.ProcessType);
        });
    }

    [Fact]
    public async Task Execute_LegacyContactInductionStatusChangedEvent_CreatesProcessAndProcessEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.DqtContactInductionStatusChangedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            InductionStatus = "InProgress",
            OldInductionStatus = "RequiredToComplete"
        });

        // Act
        await WithServiceAsync<BackfillDqtInductionAndIttProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);

            var changedEvent = Assert.IsType<DqtContactInductionStatusChangedEvent>(processEvent.Payload);
            Assert.Equal("InProgress", changedEvent.InductionStatus);
            Assert.Equal("RequiredToComplete", changedEvent.OldInductionStatus);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.PersonInductionStatusChangingInDqt, process.ProcessType);
        });
    }

    [Fact]
    public async Task Execute_LegacyInitialTeacherTrainingCreatedEvent_CreatesProcessAndProcessEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.DqtInitialTeacherTrainingCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            InitialTeacherTraining = new EventModels.DqtInitialTeacherTraining { Result = "InTraining" }
        });

        // Act
        await WithServiceAsync<BackfillDqtInductionAndIttProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);

            var createdEvent = Assert.IsType<DqtInitialTeacherTrainingCreatedEvent>(processEvent.Payload);
            Assert.Equal("InTraining", createdEvent.InitialTeacherTraining?.Result);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.InitialTeacherTrainingCreatingInDqt, process.ProcessType);
        });
    }

    [Fact]
    public async Task Execute_LegacyInitialTeacherTrainingUpdatedEvent_CreatesProcessAndProcessEventWithChanges()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.DqtInitialTeacherTrainingUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            InitialTeacherTraining = new EventModels.DqtInitialTeacherTraining { Result = "Pass" },
            OldInitialTeacherTraining = new EventModels.DqtInitialTeacherTraining { Result = "InTraining" },
            Changes = LegacyEvents.DqtInitialTeacherTrainingUpdatedEventChanges.Result
        });

        // Act
        await WithServiceAsync<BackfillDqtInductionAndIttProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);

            var updatedEvent = Assert.IsType<DqtInitialTeacherTrainingUpdatedEvent>(processEvent.Payload);
            Assert.Equal("Pass", updatedEvent.InitialTeacherTraining?.Result);
            Assert.Equal("InTraining", updatedEvent.OldInitialTeacherTraining?.Result);
            Assert.Equal(DqtInitialTeacherTrainingUpdatedEventChanges.Result, updatedEvent.Changes);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.InitialTeacherTrainingUpdatingInDqt, process.ProcessType);
        });
    }

    [Fact]
    public async Task Execute_MoreEventsThanFitInOneBatch_MigratesThemAll()
    {
        // Arrange
        // The job's batch size is 1000; go a little over it so more than one batch is needed.
        const int eventCount = 1005;

        var person = await TestData.CreatePersonAsync();

        var legacyEvents = Enumerable.Range(0, eventCount)
            .Select(i => new LegacyEvents.DqtInductionImportedEvent
            {
                EventId = Guid.NewGuid(),
                // Distinct timestamps so the (created, event_id) paging has a stable order to walk.
                CreatedUtc = TimeProvider.UtcNow.AddSeconds(i),
                RaisedBy = SystemUser.SystemUserId,
                PersonId = person.PersonId,
                Induction = CreateInduction(),
                DqtState = 0
            })
            .ToArray();

        await WithDbContextAsync(async dbContext =>
        {
            foreach (var legacyEvent in legacyEvents)
            {
                dbContext.AddEventWithoutBroadcast(legacyEvent);
            }

            await dbContext.SaveChangesAsync();
        });

        // Act
        await WithServiceAsync<BackfillDqtInductionAndIttProcessesJob>(
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
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.DqtInductionCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            Induction = CreateInduction()
        });

        await WithServiceAsync<BackfillDqtInductionAndIttProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Act
        await WithServiceAsync<BackfillDqtInductionAndIttProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvents = await dbContext.ProcessEvents.Where(pe => pe.ProcessEventId == legacyEvent.EventId).ToListAsync();
            Assert.Single(processEvents);

            var processes = await dbContext.Processes.Where(p => p.ProcessType == ProcessType.InductionCreatingInDqt).ToListAsync();
            Assert.Single(processes);
        });
    }

    [Fact]
    public async Task Execute_DryRun_DoesNotCommitChanges()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.DqtInductionCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            Induction = CreateInduction()
        });

        // Act
        await WithServiceAsync<BackfillDqtInductionAndIttProcessesJob>(
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

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.InductionMigratedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            InductionStatus = InductionStatus.InProgress,
            InductionExemptionReasonId = null,
            InductionStartDate = null,
            InductionCompletedDate = null,
            DqtInduction = CreateInduction(),
            DqtInductionStatus = "In progress"
        });

        // Act
        await WithServiceAsync<BackfillDqtInductionAndIttProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    private static EventModels.DqtInduction CreateInduction() => new()
    {
        InductionId = Guid.NewGuid(),
        StartDate = Option.Some<DateOnly?>(new DateOnly(2020, 9, 1)),
        CompletionDate = Option.None<DateOnly?>(),
        InductionStatus = Option.Some<string?>("InProgress"),
        InductionExemptionReason = Option.None<string?>()
    };

    private async Task<TEvent> AddLegacyEventAsync<TEvent>(TEvent legacyEvent) where TEvent : LegacyEvents.EventBase
    {
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(legacyEvent);
            await dbContext.SaveChangesAsync();
        });

        return legacyEvent;
    }
}

using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillDqtInitialTeacherTrainingProcessesJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_LegacyCreatedEvent_CreatesProcessAndProcessEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.DqtInitialTeacherTrainingCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            InitialTeacherTraining = new EventModels.DqtInitialTeacherTraining
            {
                InitialTeacherTrainingId = Guid.NewGuid(),
                Result = "InTraining"
            }
        });

        // Act
        await WithServiceAsync<BackfillDqtInitialTeacherTrainingProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(DqtInitialTeacherTrainingCreatedEvent), processEvent.EventName);
            Assert.Equal(person.PersonId, Assert.Single(processEvent.PersonIds));

            var createdEvent = Assert.IsType<DqtInitialTeacherTrainingCreatedEvent>(processEvent.Payload);
            Assert.Equal("InTraining", createdEvent.InitialTeacherTraining?.Result);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.InitialTeacherTrainingCreatingInDqt, process.ProcessType);
            Assert.Equal(SystemUser.SystemUserId, process.UserId);
            Assert.Equal(legacyEvent.CreatedUtc, process.CreatedOn);
            Assert.Equal(person.PersonId, Assert.Single(process.PersonIds));
        });
    }

    [Fact]
    public async Task Execute_LegacyUpdatedEvent_CreatesProcessAndProcessEventWithChangesAndOldTraining()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var ittId = Guid.NewGuid();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.DqtInitialTeacherTrainingUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            InitialTeacherTraining = new EventModels.DqtInitialTeacherTraining { InitialTeacherTrainingId = ittId, Result = "Pass" },
            OldInitialTeacherTraining = new EventModels.DqtInitialTeacherTraining { InitialTeacherTrainingId = ittId, Result = "InTraining" },
            Changes = LegacyEvents.DqtInitialTeacherTrainingUpdatedEventChanges.Result
        });

        // Act
        await WithServiceAsync<BackfillDqtInitialTeacherTrainingProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(DqtInitialTeacherTrainingUpdatedEvent), processEvent.EventName);

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
            .Select(i => new LegacyEvents.DqtInitialTeacherTrainingCreatedEvent
            {
                EventId = Guid.NewGuid(),
                // Distinct timestamps so the (created, event_id) paging has a stable order to walk.
                CreatedUtc = TimeProvider.UtcNow.AddSeconds(i),
                RaisedBy = SystemUser.SystemUserId,
                PersonId = person.PersonId,
                InitialTeacherTraining = new EventModels.DqtInitialTeacherTraining { Result = "InTraining" }
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
        await WithServiceAsync<BackfillDqtInitialTeacherTrainingProcessesJob>(
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
        var legacyEvent = await AddCreatedEventAsync();

        await WithServiceAsync<BackfillDqtInitialTeacherTrainingProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Act
        await WithServiceAsync<BackfillDqtInitialTeacherTrainingProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvents = await dbContext.ProcessEvents.Where(pe => pe.ProcessEventId == legacyEvent.EventId).ToListAsync();
            Assert.Single(processEvents);

            var processes = await dbContext.Processes.Where(p => p.ProcessType == ProcessType.InitialTeacherTrainingCreatingInDqt).ToListAsync();
            Assert.Single(processes);
        });
    }

    [Fact]
    public async Task Execute_DryRun_DoesNotCommitChanges()
    {
        // Arrange
        var legacyEvent = await AddCreatedEventAsync();

        // Act
        await WithServiceAsync<BackfillDqtInitialTeacherTrainingProcessesJob>(
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
        await WithServiceAsync<BackfillDqtInitialTeacherTrainingProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    private async Task<LegacyEvents.DqtInitialTeacherTrainingCreatedEvent> AddCreatedEventAsync()
    {
        var person = await TestData.CreatePersonAsync();

        return await AddLegacyEventAsync(new LegacyEvents.DqtInitialTeacherTrainingCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            InitialTeacherTraining = new EventModels.DqtInitialTeacherTraining { Result = "InTraining" }
        });
    }

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

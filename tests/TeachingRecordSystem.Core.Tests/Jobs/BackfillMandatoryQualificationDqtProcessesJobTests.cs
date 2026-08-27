using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillMandatoryQualificationDqtProcessesJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_LegacyDqtDeactivatedEvent_CreatesProcessAndProcessEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var dqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.MandatoryQualificationDqtDeactivatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = dqtUser,
            PersonId = person.PersonId,
            MandatoryQualification = CreateMandatoryQualification()
        });

        // Act
        await WithServiceAsync<BackfillMandatoryQualificationDqtProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(MandatoryQualificationDqtDeactivatedEvent), processEvent.EventName);
            Assert.Equal(person.PersonId, Assert.Single(processEvent.PersonIds));

            var deactivatedEvent = Assert.IsType<MandatoryQualificationDqtDeactivatedEvent>(processEvent.Payload);
            Assert.Equal(MandatoryQualificationSpecialism.DeafEducation, deactivatedEvent.MandatoryQualification.Specialism);
            Assert.Equal(MandatoryQualificationStatus.Passed, deactivatedEvent.MandatoryQualification.Status);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.MandatoryQualificationDeactivatingInDqt, process.ProcessType);
            Assert.Null(process.UserId);
            Assert.Equal(dqtUser.DqtUserId, process.DqtUserId);
            Assert.Equal(dqtUser.DqtUserName, process.DqtUserName);
            Assert.Equal(legacyEvent.CreatedUtc, process.CreatedOn);
            Assert.Equal(person.PersonId, Assert.Single(process.PersonIds));
        });
    }

    [Fact]
    public async Task Execute_LegacyDqtImportedEvent_CreatesProcessAndProcessEventWithDqtState()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.MandatoryQualificationDqtImportedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            MandatoryQualification = CreateMandatoryQualification(),
            DqtState = 1
        });

        // Act
        await WithServiceAsync<BackfillMandatoryQualificationDqtProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(MandatoryQualificationDqtImportedEvent), processEvent.EventName);

            var importedEvent = Assert.IsType<MandatoryQualificationDqtImportedEvent>(processEvent.Payload);
            Assert.Equal(1, importedEvent.DqtState);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.MandatoryQualificationImportingIntoDqt, process.ProcessType);
            Assert.Equal(SystemUser.SystemUserId, process.UserId);
        });
    }

    [Fact]
    public async Task Execute_LegacyMigratedEvent_CreatesProcessAndProcessEventWithChanges()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.MandatoryQualificationMigratedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            MandatoryQualification = CreateMandatoryQualification(),
            Changes = LegacyEvents.MandatoryQualificationMigratedEventChanges.Provider |
                LegacyEvents.MandatoryQualificationMigratedEventChanges.Specialism
        });

        // Act
        await WithServiceAsync<BackfillMandatoryQualificationDqtProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(MandatoryQualificationMigratedEvent), processEvent.EventName);

            var migratedEvent = Assert.IsType<MandatoryQualificationMigratedEvent>(processEvent.Payload);
            Assert.Equal(
                MandatoryQualificationMigratedEventChanges.Provider | MandatoryQualificationMigratedEventChanges.Specialism,
                migratedEvent.Changes);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.MandatoryQualificationMigratingFromDqt, process.ProcessType);
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
            .Select(i => new LegacyEvents.MandatoryQualificationDqtDeactivatedEvent
            {
                EventId = Guid.NewGuid(),
                // Distinct timestamps so the (created, event_id) paging has a stable order to walk.
                CreatedUtc = TimeProvider.UtcNow.AddSeconds(i),
                RaisedBy = SystemUser.SystemUserId,
                PersonId = person.PersonId,
                MandatoryQualification = CreateMandatoryQualification()
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
        await WithServiceAsync<BackfillMandatoryQualificationDqtProcessesJob>(
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
        var legacyEvent = await AddDeactivatedEventAsync();

        await WithServiceAsync<BackfillMandatoryQualificationDqtProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Act
        await WithServiceAsync<BackfillMandatoryQualificationDqtProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvents = await dbContext.ProcessEvents.Where(pe => pe.ProcessEventId == legacyEvent.EventId).ToListAsync();
            Assert.Single(processEvents);

            var processes = await dbContext.Processes.Where(p => p.ProcessType == ProcessType.MandatoryQualificationDeactivatingInDqt).ToListAsync();
            Assert.Single(processes);
        });
    }

    [Fact]
    public async Task Execute_DryRun_DoesNotCommitChanges()
    {
        // Arrange
        var legacyEvent = await AddDeactivatedEventAsync();

        // Act
        await WithServiceAsync<BackfillMandatoryQualificationDqtProcessesJob>(
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

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.MandatoryQualificationDqtReactivatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            MandatoryQualification = CreateMandatoryQualification()
        });

        // Act
        await WithServiceAsync<BackfillMandatoryQualificationDqtProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    private static EventModels.MandatoryQualification CreateMandatoryQualification() => new()
    {
        QualificationId = Guid.NewGuid(),
        Provider = new EventModels.MandatoryQualificationProvider
        {
            MandatoryQualificationProviderId = Guid.NewGuid(),
            Name = "University of Manchester"
        },
        Specialism = MandatoryQualificationSpecialism.DeafEducation,
        Status = MandatoryQualificationStatus.Passed,
        StartDate = new DateOnly(2020, 1, 1),
        EndDate = new DateOnly(2021, 1, 1)
    };

    private async Task<LegacyEvents.MandatoryQualificationDqtDeactivatedEvent> AddDeactivatedEventAsync()
    {
        var person = await TestData.CreatePersonAsync();

        return await AddLegacyEventAsync(new LegacyEvents.MandatoryQualificationDqtDeactivatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            MandatoryQualification = CreateMandatoryQualification()
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

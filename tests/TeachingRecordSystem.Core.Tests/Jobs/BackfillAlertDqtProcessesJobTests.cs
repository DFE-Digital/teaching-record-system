using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillAlertDqtProcessesJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_LegacyDeactivatedEvent_CreatesProcessAndProcessEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var alert = CreateAlert();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.AlertDqtDeactivatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            Alert = alert
        });

        // Act
        await WithServiceAsync<BackfillAlertDqtProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(AlertDqtDeactivatedEvent), processEvent.EventName);
            Assert.Equal(person.PersonId, Assert.Single(processEvent.PersonIds));

            var deactivatedEvent = Assert.IsType<AlertDqtDeactivatedEvent>(processEvent.Payload);
            Assert.Equal(person.PersonId, deactivatedEvent.PersonId);
            Assert.Equal(alert.AlertId, deactivatedEvent.Alert.AlertId);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.AlertDeactivatingInDqt, process.ProcessType);
            Assert.Equal(SystemUser.SystemUserId, process.UserId);
            Assert.Equal(legacyEvent.CreatedUtc, process.CreatedOn);
            Assert.Equal(person.PersonId, Assert.Single(process.PersonIds));
        });
    }

    [Fact]
    public async Task Execute_LegacyImportedEvent_CreatesProcessAndProcessEventWithDqtState()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var alert = CreateAlert();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.AlertDqtImportedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            Alert = alert,
            DqtState = 1
        });

        // Act
        await WithServiceAsync<BackfillAlertDqtProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(AlertDqtImportedEvent), processEvent.EventName);

            var importedEvent = Assert.IsType<AlertDqtImportedEvent>(processEvent.Payload);
            Assert.Equal(alert.AlertId, importedEvent.Alert.AlertId);
            Assert.Equal(1, importedEvent.DqtState);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.AlertImportingIntoDqt, process.ProcessType);
        });
    }

    [Fact]
    public async Task Execute_LegacyReactivatedEvent_CreatesProcessAndProcessEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var alert = CreateAlert();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.AlertDqtReactivatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            Alert = alert
        });

        // Act
        await WithServiceAsync<BackfillAlertDqtProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(AlertDqtReactivatedEvent), processEvent.EventName);

            var reactivatedEvent = Assert.IsType<AlertDqtReactivatedEvent>(processEvent.Payload);
            Assert.Equal(alert.AlertId, reactivatedEvent.Alert.AlertId);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.AlertReactivatingInDqt, process.ProcessType);
        });
    }

    [Fact]
    public async Task Execute_LegacyMigratedEvent_CreatesProcessAndProcessEventWithOldAlert()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var alert = CreateAlert();
        var oldAlert = alert with { Details = "Previous details" };

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.AlertMigratedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            Alert = alert,
            OldAlert = oldAlert
        });

        // Act
        await WithServiceAsync<BackfillAlertDqtProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(AlertMigratedEvent), processEvent.EventName);

            var migratedEvent = Assert.IsType<AlertMigratedEvent>(processEvent.Payload);
            Assert.Equal(alert.AlertId, migratedEvent.Alert.AlertId);
            Assert.Equal("Previous details", migratedEvent.OldAlert.Details);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.AlertMigratingFromDqt, process.ProcessType);
        });
    }

    [Fact]
    public async Task Execute_RunTwice_DoesNotBackfillTwice()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.AlertDqtImportedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            Alert = CreateAlert(),
            DqtState = 0
        });

        await WithServiceAsync<BackfillAlertDqtProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Act
        await WithServiceAsync<BackfillAlertDqtProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvents = await dbContext.ProcessEvents.Where(pe => pe.ProcessEventId == legacyEvent.EventId).ToListAsync();
            Assert.Single(processEvents);

            var processes = await dbContext.Processes.Where(p => p.ProcessType == ProcessType.AlertImportingIntoDqt).ToListAsync();
            Assert.Single(processes);
        });
    }

    [Fact]
    public async Task Execute_DryRun_DoesNotCommitChanges()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.AlertDqtImportedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            Alert = CreateAlert(),
            DqtState = 0
        });

        // Act
        await WithServiceAsync<BackfillAlertDqtProcessesJob>(
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

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.PersonStatusUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            Status = PersonStatus.Deactivated,
            OldStatus = PersonStatus.Active,
            Reason = null,
            ReasonDetail = null,
            AdditionalInformation = null,
            EvidenceFile = null,
            DateOfDeath = null
        });

        // Act
        await WithServiceAsync<BackfillAlertDqtProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    private static EventModels.Alert CreateAlert() => new()
    {
        AlertId = Guid.NewGuid(),
        AlertTypeId = Guid.NewGuid(),
        Details = "Some details",
        ExternalLink = null,
        StartDate = new DateOnly(2024, 1, 1),
        EndDate = null
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

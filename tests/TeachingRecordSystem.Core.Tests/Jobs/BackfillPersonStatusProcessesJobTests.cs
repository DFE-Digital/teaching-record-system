using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Jobs;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillPersonStatusProcessesJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_LegacyDeactivationEvent_CreatesPersonDeactivatingProcess()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var evidenceFile = new EventModels.File { FileId = Guid.NewGuid(), Name = "evidence.jpg" };

        var legacyEvent = await AddLegacyStatusUpdatedEventAsync(
            person,
            PersonStatus.Deactivated,
            reason: "Another reason",
            reasonDetail: "Reason detail",
            additionalInformation: "Additional information",
            evidenceFile: evidenceFile);

        // Act
        await WithServiceAsync<BackfillPersonStatusProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents
                .SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(PersonDeactivatedEvent), processEvent.EventName);

            var deactivatedEvent = Assert.IsType<PersonDeactivatedEvent>(processEvent.Payload);
            Assert.Equal(person.PersonId, deactivatedEvent.PersonId);
            Assert.Equal(PersonDeactivatedEventChanges.PersonStatus, deactivatedEvent.Changes);
            Assert.Null(deactivatedEvent.MergedWithPersonId);
            Assert.Null(deactivatedEvent.DateOfDeath);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.PersonDeactivating, process.ProcessType);
            Assert.Equal(SystemUser.SystemUserId, process.UserId);
            Assert.Equal(legacyEvent.CreatedUtc, process.CreatedOn);
            Assert.Equal(person.PersonId, Assert.Single(process.PersonIds));

            var changeReason = Assert.IsType<ChangeReasonWithDetailsAndEvidence>(process.ChangeReason);
            Assert.Equal("Another reason", changeReason.Reason);
            Assert.Equal("Reason detail", changeReason.Details);
            Assert.Equal("Additional information", changeReason.AdditionalInformation);
            Assert.Equal(evidenceFile.FileId, changeReason.EvidenceFile?.FileId);
        });
    }

    [Fact]
    public async Task Execute_LegacyDeactivationEventWithDateOfDeath_FlagsDateOfDeathChange()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var dateOfDeath = new DateOnly(2024, 5, 6);

        var legacyEvent = await AddLegacyStatusUpdatedEventAsync(person, PersonStatus.Deactivated, dateOfDeath: dateOfDeath);

        // Act
        await WithServiceAsync<BackfillPersonStatusProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleAsync(pe => pe.ProcessEventId == legacyEvent.EventId);

            var deactivatedEvent = Assert.IsType<PersonDeactivatedEvent>(processEvent.Payload);
            Assert.Equal(
                PersonDeactivatedEventChanges.PersonStatus | PersonDeactivatedEventChanges.DateOfDeath,
                deactivatedEvent.Changes);
            Assert.Equal(dateOfDeath, deactivatedEvent.DateOfDeath);
        });
    }

    [Fact]
    public async Task Execute_LegacyReactivationEvent_CreatesPersonReactivatingProcess()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var legacyEvent = await AddLegacyStatusUpdatedEventAsync(person, PersonStatus.Active);

        // Act
        await WithServiceAsync<BackfillPersonStatusProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents
                .SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(PersonReactivatedEvent), processEvent.EventName);

            var reactivatedEvent = Assert.IsType<PersonReactivatedEvent>(processEvent.Payload);
            Assert.Equal(person.PersonId, reactivatedEvent.PersonId);
            Assert.Equal(PersonReactivatedEventChanges.PersonStatus, reactivatedEvent.Changes);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.PersonReactivating, process.ProcessType);
        });
    }

    [Fact]
    public async Task Execute_LegacyEventThatAlreadyHasAProcess_IsLeftAlone()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var legacyEvent = await AddLegacyStatusUpdatedEventAsync(person, PersonStatus.Deactivated);

        await TestData.CreateProcessAsync(
            ProcessType.PersonDeactivating,
            SystemUser.SystemUserId,
            changeReason: null,
            new PersonDeactivatedEvent
            {
                EventId = legacyEvent.EventId,
                PersonId = person.PersonId,
                Changes = PersonDeactivatedEventChanges.PersonStatus,
                MergedWithPersonId = null,
                DateOfDeath = null
            });

        // Act
        await WithServiceAsync<BackfillPersonStatusProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processes = await dbContext.Processes
                .Where(p => p.PersonIds.Contains(person.PersonId) && p.ProcessType == ProcessType.PersonDeactivating)
                .ToListAsync();
            Assert.Single(processes);
        });
    }

    [Fact]
    public async Task Execute_RunTwice_DoesNotCreateDuplicateProcesses()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await AddLegacyStatusUpdatedEventAsync(person, PersonStatus.Deactivated);

        await WithServiceAsync<BackfillPersonStatusProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Act
        await WithServiceAsync<BackfillPersonStatusProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processes = await dbContext.Processes
                .Where(p => p.PersonIds.Contains(person.PersonId) && p.ProcessType == ProcessType.PersonDeactivating)
                .ToListAsync();
            Assert.Single(processes);
        });
    }

    [Fact]
    public async Task Execute_UnrelatedLegacyEvent_IsLeftAlone()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        await AddLegacyEventAsync(new LegacyEvents.TrnAllocatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            Trn = person.Trn!
        });

        // Act
        await WithServiceAsync<BackfillPersonStatusProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processes = await dbContext.Processes
                .Where(p => p.PersonIds.Contains(person.PersonId))
                .ToListAsync();
            Assert.Empty(processes);
        });
    }

    private Task<LegacyEvents.PersonStatusUpdatedEvent> AddLegacyStatusUpdatedEventAsync(
        Person person,
        PersonStatus status,
        string? reason = null,
        string? reasonDetail = null,
        string? additionalInformation = null,
        EventModels.File? evidenceFile = null,
        DateOnly? dateOfDeath = null) =>
        AddLegacyEventAsync(new LegacyEvents.PersonStatusUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            Status = status,
            OldStatus = status is PersonStatus.Deactivated ? PersonStatus.Active : PersonStatus.Deactivated,
            Reason = reason,
            ReasonDetail = reasonDetail,
            AdditionalInformation = additionalInformation,
            EvidenceFile = evidenceFile,
            DateOfDeath = dateOfDeath
        });

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

using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Jobs;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillPersonMergeProcessesJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_LegacyMergedEvent_CreatesProcessWithDeactivatedAndDetailsUpdatedEvents()
    {
        // Arrange
        var retainedPerson = await TestData.CreatePersonAsync();
        var deactivatedPerson = await TestData.CreatePersonAsync();
        var newLastName = TestData.GenerateChangedLastName(retainedPerson.LastName);
        var comments = TestData.GenerateLoremIpsum();
        var evidenceFile = new EventModels.File { FileId = Guid.NewGuid(), Name = "evidence.jpg" };

        var legacyEvent = await AddLegacyMergedEventAsync(
            retainedPerson,
            deactivatedPerson,
            LegacyEvents.PersonsMergedEventChanges.LastName,
            CreatePersonDetails(retainedPerson.FirstName, retainedPerson.MiddleName, newLastName, retainedPerson.DateOfBirth),
            CreatePersonDetails(retainedPerson.FirstName, retainedPerson.MiddleName, retainedPerson.LastName, retainedPerson.DateOfBirth),
            comments,
            evidenceFile);

        // Act
        await WithServiceAsync<BackfillPersonMergeProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var deactivatedProcessEvent = await dbContext.ProcessEvents
                .SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(deactivatedProcessEvent);
            Assert.Equal(nameof(PersonDeactivatedEvent), deactivatedProcessEvent.EventName);

            var deactivatedEvent = Assert.IsType<PersonDeactivatedEvent>(deactivatedProcessEvent.Payload);
            Assert.Equal(deactivatedPerson.PersonId, deactivatedEvent.PersonId);
            Assert.Equal(retainedPerson.PersonId, deactivatedEvent.MergedWithPersonId);
            Assert.Equal(PersonDeactivatedEventChanges.MergedWithPersonId, deactivatedEvent.Changes);
            Assert.Null(deactivatedEvent.DateOfDeath);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == deactivatedProcessEvent.ProcessId);
            Assert.Equal(ProcessType.PersonMerging, process.ProcessType);
            Assert.Equal(SystemUser.SystemUserId, process.UserId);
            Assert.Equal(legacyEvent.CreatedUtc, process.CreatedOn);
            Assert.Contains(retainedPerson.PersonId, process.PersonIds);
            Assert.Contains(deactivatedPerson.PersonId, process.PersonIds);

            var changeReason = Assert.IsType<ChangeReasonWithDetailsAndEvidence>(process.ChangeReason);
            Assert.Null(changeReason.Reason);
            Assert.Equal(comments, changeReason.Details);
            Assert.Equal(evidenceFile.FileId, changeReason.EvidenceFile?.FileId);
            Assert.Equal(evidenceFile.Name, changeReason.EvidenceFile?.Name);
            Assert.Null(changeReason.AdditionalInformation);

            var processEvents = await dbContext.ProcessEvents
                .Where(pe => pe.ProcessId == process.ProcessId)
                .ToListAsync();
            Assert.Equal(2, processEvents.Count);

            var detailsUpdatedEvent = Assert.IsType<PersonDetailsUpdatedEvent>(
                Assert.Single(processEvents, pe => pe.Payload is PersonDetailsUpdatedEvent).Payload);
            Assert.Equal(retainedPerson.PersonId, detailsUpdatedEvent.PersonId);
            Assert.Equal(PersonDetailsUpdatedEventChanges.LastName, detailsUpdatedEvent.Changes);
            Assert.Equal(newLastName, detailsUpdatedEvent.PersonDetails.LastName);
            Assert.Equal(retainedPerson.LastName, detailsUpdatedEvent.OldPersonDetails.LastName);
        });
    }

    [Fact]
    public async Task Execute_LegacyMergedEventThatChangedNothing_CreatesProcessWithoutDetailsUpdatedEvent()
    {
        // Arrange
        var retainedPerson = await TestData.CreatePersonAsync();
        var deactivatedPerson = await TestData.CreatePersonAsync();
        var details = CreatePersonDetails(retainedPerson.FirstName, retainedPerson.MiddleName, retainedPerson.LastName, retainedPerson.DateOfBirth);

        var legacyEvent = await AddLegacyMergedEventAsync(
            retainedPerson,
            deactivatedPerson,
            LegacyEvents.PersonsMergedEventChanges.None,
            details,
            details,
            comments: null,
            evidenceFile: null);

        // Act
        await WithServiceAsync<BackfillPersonMergeProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var deactivatedProcessEvent = await dbContext.ProcessEvents
                .SingleAsync(pe => pe.ProcessEventId == legacyEvent.EventId);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == deactivatedProcessEvent.ProcessId);
            Assert.Contains(retainedPerson.PersonId, process.PersonIds);
            Assert.Contains(deactivatedPerson.PersonId, process.PersonIds);

            var changeReason = Assert.IsType<ChangeReasonWithDetailsAndEvidence>(process.ChangeReason);
            Assert.Null(changeReason.Details);
            Assert.Null(changeReason.EvidenceFile);

            var processEvents = await dbContext.ProcessEvents
                .Where(pe => pe.ProcessId == process.ProcessId)
                .ToListAsync();
            Assert.Single(processEvents);
            Assert.DoesNotContain(processEvents, pe => pe.Payload is PersonDetailsUpdatedEvent);
        });
    }

    [Fact]
    public async Task Execute_LegacyEventThatAlreadyHasAProcess_IsLeftAlone()
    {
        // Arrange
        var retainedPerson = await TestData.CreatePersonAsync();
        var deactivatedPerson = await TestData.CreatePersonAsync();
        var details = CreatePersonDetails(retainedPerson.FirstName, retainedPerson.MiddleName, retainedPerson.LastName, retainedPerson.DateOfBirth);

        var legacyEvent = await AddLegacyMergedEventAsync(
            retainedPerson,
            deactivatedPerson,
            LegacyEvents.PersonsMergedEventChanges.None,
            details,
            details,
            comments: null,
            evidenceFile: null);

        // The dual-write era gave the legacy event and its PersonDeactivatedEvent the same id.
        await TestData.CreateProcessAsync(
            ProcessType.PersonMerging,
            SystemUser.SystemUserId,
            changeReason: null,
            new PersonDeactivatedEvent
            {
                EventId = legacyEvent.EventId,
                PersonId = deactivatedPerson.PersonId,
                MergedWithPersonId = retainedPerson.PersonId,
                Changes = PersonDeactivatedEventChanges.MergedWithPersonId,
                DateOfDeath = null
            });

        // Act
        await WithServiceAsync<BackfillPersonMergeProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processes = await dbContext.Processes
                .Where(p => p.ProcessType == ProcessType.PersonMerging)
                .ToListAsync();
            Assert.Single(processes);
        });
    }

    [Fact]
    public async Task Execute_RunTwice_DoesNotCreateDuplicateProcesses()
    {
        // Arrange
        var retainedPerson = await TestData.CreatePersonAsync();
        var deactivatedPerson = await TestData.CreatePersonAsync();
        var details = CreatePersonDetails(retainedPerson.FirstName, retainedPerson.MiddleName, retainedPerson.LastName, retainedPerson.DateOfBirth);

        await AddLegacyMergedEventAsync(
            retainedPerson,
            deactivatedPerson,
            LegacyEvents.PersonsMergedEventChanges.None,
            details,
            details,
            comments: null,
            evidenceFile: null);

        await WithServiceAsync<BackfillPersonMergeProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Act
        await WithServiceAsync<BackfillPersonMergeProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processes = await dbContext.Processes
                .Where(p => p.ProcessType == ProcessType.PersonMerging)
                .ToListAsync();
            Assert.Single(processes);
        });
    }

    [Fact]
    public async Task Execute_UnrelatedLegacyEvent_IsLeftAlone()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        await AddLegacyEventAsync(new LegacyEvents.PersonStatusUpdatedEvent
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
        await WithServiceAsync<BackfillPersonMergeProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processes = await dbContext.Processes
                .Where(p => p.ProcessType == ProcessType.PersonMerging)
                .ToListAsync();
            Assert.Empty(processes);
        });
    }

    private Task<LegacyEvents.PersonsMergedEvent> AddLegacyMergedEventAsync(
        Person retainedPerson,
        Person deactivatedPerson,
        LegacyEvents.PersonsMergedEventChanges changes,
        EventModels.PersonDetails personAttributes,
        EventModels.PersonDetails oldPersonAttributes,
        string? comments,
        EventModels.File? evidenceFile) =>
        AddLegacyEventAsync(new LegacyEvents.PersonsMergedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = retainedPerson.PersonId,
            PersonTrn = retainedPerson.Trn!,
            SecondaryPersonId = deactivatedPerson.PersonId,
            SecondaryPersonTrn = deactivatedPerson.Trn!,
            SecondaryPersonStatus = PersonStatus.Deactivated,
            PersonAttributes = personAttributes,
            OldPersonAttributes = oldPersonAttributes,
            Changes = changes,
            Comments = comments,
            EvidenceFile = evidenceFile
        });

    private static EventModels.PersonDetails CreatePersonDetails(
        string firstName,
        string? middleName,
        string lastName,
        DateOnly? dateOfBirth) => new()
        {
            FirstName = firstName,
            MiddleName = middleName ?? string.Empty,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            EmailAddress = null,
            NationalInsuranceNumber = null,
            Gender = null
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

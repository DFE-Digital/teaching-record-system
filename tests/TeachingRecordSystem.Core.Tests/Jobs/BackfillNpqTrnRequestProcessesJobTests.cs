using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillNpqTrnRequestProcessesJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_LegacyResolvedEventThatCreatedRecord_CreatesProcessWithSupportTaskTrnRequestAndPersonCreatedEvents()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var personAttributes = CreatePersonDetails(person.FirstName, person.MiddleName, person.LastName, person.DateOfBirth);

        var legacyEvent = await AddLegacyResolvedEventAsync(
            person.PersonId,
            applicationUser.UserId,
            LegacyEvents.NpqTrnRequestResolvedReason.RecordCreated,
            LegacyEvents.NpqTrnRequestSupportTaskResolvedEventChanges.Status,
            personAttributes,
            oldPersonAttributes: null,
            comments: null);

        // Act
        await WithServiceAsync<BackfillNpqTrnRequestProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var supportTaskUpdatedProcessEvent = await dbContext.ProcessEvents
                .SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(supportTaskUpdatedProcessEvent);
            Assert.Equal(nameof(SupportTaskUpdatedEvent), supportTaskUpdatedProcessEvent.EventName);

            var supportTaskUpdatedEvent = Assert.IsType<SupportTaskUpdatedEvent>(supportTaskUpdatedProcessEvent.Payload);
            Assert.Equal("TEST-ST-1", supportTaskUpdatedEvent.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, supportTaskUpdatedEvent.SupportTask.Status);
            Assert.Equal(SupportTaskStatus.Open, supportTaskUpdatedEvent.OldSupportTask.Status);
            Assert.Null(supportTaskUpdatedEvent.RejectionReason);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == supportTaskUpdatedProcessEvent.ProcessId);
            Assert.Equal(ProcessType.NpqTrnRequestApproving, process.ProcessType);
            Assert.Equal(SystemUser.SystemUserId, process.UserId);
            Assert.Equal(legacyEvent.CreatedUtc, process.CreatedOn);
            Assert.Equal(person.PersonId, Assert.Single(process.PersonIds));
            Assert.Equal("TEST-ST-1", Assert.Single(process.SupportTaskReferences));

            var processEvents = await dbContext.ProcessEvents
                .Where(pe => pe.ProcessId == process.ProcessId)
                .ToListAsync();
            Assert.Equal(3, processEvents.Count);

            var trnRequestUpdatedEvent = Assert.IsType<TrnRequestUpdatedEvent>(
                Assert.Single(processEvents, pe => pe.Payload is TrnRequestUpdatedEvent).Payload);
            Assert.Equal(applicationUser.UserId, trnRequestUpdatedEvent.SourceApplicationUserId);
            Assert.Equal("TEST-TRN-1", trnRequestUpdatedEvent.RequestId);
            Assert.Equal(TrnRequestUpdatedChanges.Status | TrnRequestUpdatedChanges.ResolvedPersonId, trnRequestUpdatedEvent.Changes);
            Assert.Equal(TrnRequestStatus.Completed, trnRequestUpdatedEvent.TrnRequest.Status);
            Assert.Equal(person.PersonId, trnRequestUpdatedEvent.TrnRequest.ResolvedPersonId);
            Assert.Equal(TrnRequestStatus.Pending, trnRequestUpdatedEvent.OldTrnRequest.Status);
            Assert.Null(trnRequestUpdatedEvent.OldTrnRequest.ResolvedPersonId);

            var personCreatedEvent = Assert.IsType<PersonCreatedEvent>(
                Assert.Single(processEvents, pe => pe.Payload is PersonCreatedEvent).Payload);
            Assert.Equal(person.PersonId, personCreatedEvent.PersonId);
            Assert.Equal(person.FirstName, personCreatedEvent.Details.FirstName);
            Assert.Equal("TEST-TRN-1", personCreatedEvent.TrnRequestMetadata?.RequestId);

            Assert.DoesNotContain(processEvents, pe => pe.Payload is PersonDetailsUpdatedEvent);
        });
    }

    [Fact]
    public async Task Execute_LegacyResolvedEventThatMergedRecords_CreatesProcessWithPersonDetailsUpdatedEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var newEmailAddress = TestData.GenerateUniqueEmail();
        var comments = TestData.GenerateLoremIpsum();

        var personAttributes = CreatePersonDetails(person.FirstName, person.MiddleName, person.LastName, person.DateOfBirth, newEmailAddress);
        var oldPersonAttributes = CreatePersonDetails(person.FirstName, person.MiddleName, person.LastName, person.DateOfBirth);

        var legacyEvent = await AddLegacyResolvedEventAsync(
            person.PersonId,
            applicationUser.UserId,
            LegacyEvents.NpqTrnRequestResolvedReason.RecordMerged,
            LegacyEvents.NpqTrnRequestSupportTaskResolvedEventChanges.Status |
                LegacyEvents.NpqTrnRequestSupportTaskResolvedEventChanges.PersonEmailAddress,
            personAttributes,
            oldPersonAttributes,
            comments);

        // Act
        await WithServiceAsync<BackfillNpqTrnRequestProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var supportTaskUpdatedProcessEvent = await dbContext.ProcessEvents
                .SingleAsync(pe => pe.ProcessEventId == legacyEvent.EventId);

            var supportTaskUpdatedEvent = Assert.IsType<SupportTaskUpdatedEvent>(supportTaskUpdatedProcessEvent.Payload);
            Assert.Equal(comments, supportTaskUpdatedEvent.Comments);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == supportTaskUpdatedProcessEvent.ProcessId);
            Assert.Equal(ProcessType.NpqTrnRequestApproving, process.ProcessType);

            var processEvents = await dbContext.ProcessEvents
                .Where(pe => pe.ProcessId == process.ProcessId)
                .ToListAsync();
            Assert.Equal(3, processEvents.Count);

            var personDetailsUpdatedEvent = Assert.IsType<PersonDetailsUpdatedEvent>(
                Assert.Single(processEvents, pe => pe.Payload is PersonDetailsUpdatedEvent).Payload);
            Assert.Equal(person.PersonId, personDetailsUpdatedEvent.PersonId);
            Assert.Equal(PersonDetailsUpdatedEventChanges.EmailAddress, personDetailsUpdatedEvent.Changes);
            Assert.Equal(newEmailAddress, personDetailsUpdatedEvent.PersonDetails.EmailAddress);
            Assert.Null(personDetailsUpdatedEvent.OldPersonDetails.EmailAddress);

            Assert.DoesNotContain(processEvents, pe => pe.Payload is PersonCreatedEvent);
        });
    }

    [Fact]
    public async Task Execute_LegacyResolvedEventThatMergedRecordsWithoutChangingThem_CreatesProcessWithoutPersonEventButStillOnThePerson()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var personAttributes = CreatePersonDetails(person.FirstName, person.MiddleName, person.LastName, person.DateOfBirth);

        var legacyEvent = await AddLegacyResolvedEventAsync(
            person.PersonId,
            applicationUser.UserId,
            LegacyEvents.NpqTrnRequestResolvedReason.RecordMerged,
            LegacyEvents.NpqTrnRequestSupportTaskResolvedEventChanges.Status,
            personAttributes,
            oldPersonAttributes: personAttributes,
            comments: null,
            supportTaskPersonId: null);

        // Act
        await WithServiceAsync<BackfillNpqTrnRequestProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var supportTaskUpdatedProcessEvent = await dbContext.ProcessEvents
                .SingleAsync(pe => pe.ProcessEventId == legacyEvent.EventId);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == supportTaskUpdatedProcessEvent.ProcessId);
            Assert.Equal(person.PersonId, Assert.Single(process.PersonIds));

            var processEvents = await dbContext.ProcessEvents
                .Where(pe => pe.ProcessId == process.ProcessId)
                .ToListAsync();
            Assert.Equal(2, processEvents.Count);
            Assert.DoesNotContain(processEvents, pe => pe.Payload is PersonCreatedEvent or PersonDetailsUpdatedEvent);
        });
    }

    [Fact]
    public async Task Execute_LegacyRejectedEvent_CreatesRejectingProcessWithSupportTaskAndTrnRequestEvents()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var rejectionReason = "Evidence does not match the request";

        var legacyEvent = await AddLegacyRejectedEventAsync(applicationUser.UserId, rejectionReason);

        // Act
        await WithServiceAsync<BackfillNpqTrnRequestProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var supportTaskUpdatedProcessEvent = await dbContext.ProcessEvents
                .SingleAsync(pe => pe.ProcessEventId == legacyEvent.EventId);

            var supportTaskUpdatedEvent = Assert.IsType<SupportTaskUpdatedEvent>(supportTaskUpdatedProcessEvent.Payload);
            Assert.Equal(rejectionReason, supportTaskUpdatedEvent.RejectionReason);
            Assert.Null(supportTaskUpdatedEvent.Comments);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == supportTaskUpdatedProcessEvent.ProcessId);
            Assert.Equal(ProcessType.NpqTrnRequestRejecting, process.ProcessType);
            Assert.Equal(legacyEvent.CreatedUtc, process.CreatedOn);
            Assert.Empty(process.PersonIds);
            Assert.Equal("TEST-ST-1", Assert.Single(process.SupportTaskReferences));

            var processEvents = await dbContext.ProcessEvents
                .Where(pe => pe.ProcessId == process.ProcessId)
                .ToListAsync();
            Assert.Equal(2, processEvents.Count);

            var trnRequestUpdatedEvent = Assert.IsType<TrnRequestUpdatedEvent>(
                Assert.Single(processEvents, pe => pe.Payload is TrnRequestUpdatedEvent).Payload);
            Assert.Equal(TrnRequestUpdatedChanges.Status, trnRequestUpdatedEvent.Changes);
            Assert.Equal(TrnRequestStatus.Rejected, trnRequestUpdatedEvent.TrnRequest.Status);
            Assert.Equal(TrnRequestStatus.Pending, trnRequestUpdatedEvent.OldTrnRequest.Status);
        });
    }

    [Fact]
    public async Task Execute_LegacyEventThatAlreadyHasAProcess_IsLeftAlone()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var personAttributes = CreatePersonDetails(person.FirstName, person.MiddleName, person.LastName, person.DateOfBirth);

        var legacyEvent = await AddLegacyResolvedEventAsync(
            person.PersonId,
            applicationUser.UserId,
            LegacyEvents.NpqTrnRequestResolvedReason.RecordCreated,
            LegacyEvents.NpqTrnRequestSupportTaskResolvedEventChanges.Status,
            personAttributes,
            oldPersonAttributes: null,
            comments: null);

        // The dual-write era gave the legacy event and its process event the same id.
        await TestData.CreateProcessAsync(
            ProcessType.NpqTrnRequestApproving,
            SystemUser.SystemUserId,
            changeReason: null,
            new PersonCreatedEvent
            {
                EventId = legacyEvent.EventId,
                PersonId = person.PersonId,
                Details = personAttributes,
                TrnRequestMetadata = null
            });

        // Act
        await WithServiceAsync<BackfillNpqTrnRequestProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processes = await dbContext.Processes
                .Where(p => p.ProcessType == ProcessType.NpqTrnRequestApproving)
                .ToListAsync();
            Assert.Single(processes);
        });
    }

    [Fact]
    public async Task Execute_RunTwice_DoesNotCreateDuplicateProcesses()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var personAttributes = CreatePersonDetails(person.FirstName, person.MiddleName, person.LastName, person.DateOfBirth);

        await AddLegacyResolvedEventAsync(
            person.PersonId,
            applicationUser.UserId,
            LegacyEvents.NpqTrnRequestResolvedReason.RecordCreated,
            LegacyEvents.NpqTrnRequestSupportTaskResolvedEventChanges.Status,
            personAttributes,
            oldPersonAttributes: null,
            comments: null);

        await WithServiceAsync<BackfillNpqTrnRequestProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Act
        await WithServiceAsync<BackfillNpqTrnRequestProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processes = await dbContext.Processes
                .Where(p => p.ProcessType == ProcessType.NpqTrnRequestApproving)
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
        await WithServiceAsync<BackfillNpqTrnRequestProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processes = await dbContext.Processes
                .Where(p => p.ProcessType == ProcessType.NpqTrnRequestApproving || p.ProcessType == ProcessType.NpqTrnRequestRejecting)
                .ToListAsync();
            Assert.Empty(processes);
        });
    }

    private Task<LegacyEvents.NpqTrnRequestSupportTaskResolvedEvent> AddLegacyResolvedEventAsync(
        Guid personId,
        Guid applicationUserId,
        LegacyEvents.NpqTrnRequestResolvedReason changeReason,
        LegacyEvents.NpqTrnRequestSupportTaskResolvedEventChanges changes,
        EventModels.PersonDetails personAttributes,
        EventModels.PersonDetails? oldPersonAttributes,
        string? comments,
        Guid? supportTaskPersonId = null)
    {
        var oldSupportTask = CreateSupportTask(supportTaskPersonId, SupportTaskStatus.Open, outcome: null);

        return AddLegacyEventAsync(new LegacyEvents.NpqTrnRequestSupportTaskResolvedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = personId,
            RequestData = CreateRequestData(applicationUserId, personId),
            ChangeReason = changeReason,
            Changes = changes,
            PersonAttributes = personAttributes,
            OldPersonAttributes = oldPersonAttributes,
            Comments = comments,
            SupportTask = CreateSupportTask(
                supportTaskPersonId,
                SupportTaskStatus.Closed,
                SupportTaskOutcome.NpqTrnRequest_ResolvedWithNewPerson),
            OldSupportTask = oldSupportTask
        });
    }

    private Task<LegacyEvents.NpqTrnRequestSupportTaskRejectedEvent> AddLegacyRejectedEventAsync(
        Guid applicationUserId,
        string rejectionReason) =>
        AddLegacyEventAsync(new LegacyEvents.NpqTrnRequestSupportTaskRejectedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            RequestData = CreateRequestData(applicationUserId, resolvedPersonId: null),
            RejectionReason = rejectionReason,
            SupportTask = CreateSupportTask(personId: null, SupportTaskStatus.Closed, SupportTaskOutcome.NpqTrnRequest_Rejected),
            OldSupportTask = CreateSupportTask(personId: null, SupportTaskStatus.Open, outcome: null)
        });

    private static EventModels.SupportTask CreateSupportTask(
        Guid? personId,
        SupportTaskStatus status,
        SupportTaskOutcome? outcome) => new()
        {
            SupportTaskReference = "TEST-ST-1",
            SupportTaskType = SupportTaskType.NpqTrnRequest,
            Status = status,
            OneLoginUserSubject = null,
            PersonId = personId,
            Data = new NpqTrnRequestData(),
            SourceApplicationUserId = null,
            ResolveJourneySavedState = null,
            AssignedToUserId = null,
            ZendeskTickets = [],
            Outcome = outcome
        };

    private EventModels.TrnRequestMetadata CreateRequestData(Guid applicationUserId, Guid? resolvedPersonId) => new()
    {
        ApplicationUserId = applicationUserId,
        RequestId = "TEST-TRN-1",
        CreatedOn = TimeProvider.UtcNow,
        IdentityVerified = null,
        EmailAddress = null,
        OneLoginUserSubject = null,
        FirstName = "Megan",
        MiddleName = "Thee",
        LastName = "Stallion",
        PreviousFirstName = null,
        PreviousLastName = null,
        Name = ["Megan", "Thee", "Stallion"],
        DateOfBirth = new DateOnly(1990, 1, 1),
        PotentialDuplicate = null,
        NationalInsuranceNumber = null,
        Gender = null,
        AddressLine1 = null,
        AddressLine2 = null,
        AddressLine3 = null,
        City = null,
        Postcode = null,
        Country = null,
        TrnToken = null,
        // The legacy events snapshotted the request before its status moved, so leave both fields as they were.
        ResolvedPersonId = resolvedPersonId,
        Matches = null,
        NpqApplicationId = null,
        NpqEvidenceFileId = null,
        NpqEvidenceFileName = null,
        NpqName = null,
        NpqTrainingProvider = null,
        NpqWorkingInEducationalSetting = null,
        Status = TrnRequestStatus.Pending
    };

    private static EventModels.PersonDetails CreatePersonDetails(
        string firstName,
        string? middleName,
        string lastName,
        DateOnly? dateOfBirth,
        string? emailAddress = null) => new()
        {
            FirstName = firstName,
            MiddleName = middleName ?? string.Empty,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            EmailAddress = emailAddress,
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

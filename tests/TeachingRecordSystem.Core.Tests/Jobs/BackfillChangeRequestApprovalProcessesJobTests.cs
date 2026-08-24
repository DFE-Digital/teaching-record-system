using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillChangeRequestApprovalProcessesJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_LegacyNameApprovedEvent_CreatesProcessWithSupportTaskPersonDetailsAndEmailEvents()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var newFirstName = TestData.GenerateChangedFirstName(person.FirstName);
        var newMiddleName = TestData.GenerateChangedMiddleName(person.MiddleName);
        var newLastName = TestData.GenerateChangedLastName(person.LastName);
        var requestEmailAddress = TestData.GenerateUniqueEmail();

        var dbSupportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            person.PersonId,
            b => b
                .WithFirstName(newFirstName)
                .WithMiddleName(newMiddleName)
                .WithLastName(newLastName)
                .WithEmailAddress(requestEmailAddress));

        var oldSupportTask = EventModels.SupportTask.FromModel(dbSupportTask);
        var supportTask = oldSupportTask with
        {
            Status = SupportTaskStatus.Closed,
            Outcome = SupportTaskOutcome.ChangeNameRequest_Approved
        };

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.ChangeNameRequestSupportTaskApprovedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            RequestData = EventModels.ChangeNameRequestData.FromModel(dbSupportTask.GetData<ChangeNameRequestData>()),
            Changes = LegacyEvents.ChangeNameRequestSupportTaskApprovedEventChanges.NameChange,
            PersonAttributes = CreatePersonDetails(newFirstName, newMiddleName, newLastName, person.DateOfBirth),
            OldPersonAttributes = CreatePersonDetails(person.FirstName, person.MiddleName, person.LastName, person.DateOfBirth),
            SupportTask = supportTask,
            OldSupportTask = oldSupportTask
        });

        // Act
        await WithServiceAsync<BackfillChangeRequestApprovalProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var supportTaskUpdatedProcessEvent = await dbContext.ProcessEvents
                .SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(supportTaskUpdatedProcessEvent);
            Assert.Equal(nameof(SupportTaskUpdatedEvent), supportTaskUpdatedProcessEvent.EventName);

            var supportTaskUpdatedEvent = Assert.IsType<SupportTaskUpdatedEvent>(supportTaskUpdatedProcessEvent.Payload);
            Assert.Equal(dbSupportTask.SupportTaskReference, supportTaskUpdatedEvent.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, supportTaskUpdatedEvent.SupportTask.Status);
            Assert.Equal(SupportTaskStatus.Open, supportTaskUpdatedEvent.OldSupportTask.Status);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == supportTaskUpdatedProcessEvent.ProcessId);
            Assert.Equal(ProcessType.ChangeOfNameRequestApproving, process.ProcessType);
            Assert.Equal(SystemUser.SystemUserId, process.UserId);
            Assert.Equal(legacyEvent.CreatedUtc, process.CreatedOn);
            Assert.Equal(person.PersonId, Assert.Single(process.PersonIds));
            Assert.Equal(dbSupportTask.SupportTaskReference, Assert.Single(process.SupportTaskReferences));

            var processEvents = await dbContext.ProcessEvents
                .Where(pe => pe.ProcessId == process.ProcessId)
                .ToListAsync();
            Assert.Equal(3, processEvents.Count);

            var personDetailsUpdatedEvent = Assert.IsType<PersonDetailsUpdatedEvent>(
                Assert.Single(processEvents, pe => pe.Payload is PersonDetailsUpdatedEvent).Payload);
            Assert.Equal(person.PersonId, personDetailsUpdatedEvent.PersonId);
            Assert.Equal(PersonDetailsUpdatedEventChanges.NameChange, personDetailsUpdatedEvent.Changes);
            Assert.Equal(newFirstName, personDetailsUpdatedEvent.PersonDetails.FirstName);
            Assert.Equal(person.FirstName, personDetailsUpdatedEvent.OldPersonDetails.FirstName);

            var emailSentEvent = Assert.IsType<EmailSentEvent>(
                Assert.Single(processEvents, pe => pe.Payload is EmailSentEvent).Payload);
            Assert.Equal(person.PersonId, emailSentEvent.PersonId);
            Assert.Equal(EmailTemplateIds.GetAnIdentityChangeOfNameApprovedEmailConfirmation, emailSentEvent.Email.TemplateId);
            Assert.Equal(requestEmailAddress, emailSentEvent.Email.EmailAddress);
            Assert.Equal(newFirstName, emailSentEvent.Email.Personalization[ChangeRequestEmailConstants.FirstNameEmailPersonalisationKey]);
        });
    }

    [Fact]
    public async Task Execute_LegacyDateOfBirthApprovedEvent_CreatesProcessWithSupportTaskPersonDetailsAndEmailEvents()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var newDateOfBirth = TestData.GenerateChangedDateOfBirth(person.DateOfBirth!.Value);
        var requestEmailAddress = TestData.GenerateUniqueEmail();

        var dbSupportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
            person.PersonId,
            b => b
                .WithDateOfBirth(newDateOfBirth)
                .WithEmailAddress(requestEmailAddress));

        var oldSupportTask = EventModels.SupportTask.FromModel(dbSupportTask);
        var supportTask = oldSupportTask with
        {
            Status = SupportTaskStatus.Closed,
            Outcome = SupportTaskOutcome.ChangeDateOfBirthRequest_Approved
        };

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.ChangeDateOfBirthRequestSupportTaskApprovedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            RequestData = EventModels.ChangeDateOfBirthRequestData.FromModel(dbSupportTask.GetData<ChangeDateOfBirthRequestData>()),
            Changes = LegacyEvents.ChangeDateOfBirthRequestSupportTaskApprovedEventChanges.DateOfBirth,
            PersonAttributes = CreatePersonDetails(person.FirstName, person.MiddleName, person.LastName, newDateOfBirth),
            OldPersonAttributes = CreatePersonDetails(person.FirstName, person.MiddleName, person.LastName, person.DateOfBirth),
            SupportTask = supportTask,
            OldSupportTask = oldSupportTask
        });

        // Act
        await WithServiceAsync<BackfillChangeRequestApprovalProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var supportTaskUpdatedProcessEvent = await dbContext.ProcessEvents
                .SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(supportTaskUpdatedProcessEvent);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == supportTaskUpdatedProcessEvent.ProcessId);
            Assert.Equal(ProcessType.ChangeOfDateOfBirthRequestApproving, process.ProcessType);

            var processEvents = await dbContext.ProcessEvents
                .Where(pe => pe.ProcessId == process.ProcessId)
                .ToListAsync();
            Assert.Equal(3, processEvents.Count);

            var personDetailsUpdatedEvent = Assert.IsType<PersonDetailsUpdatedEvent>(
                Assert.Single(processEvents, pe => pe.Payload is PersonDetailsUpdatedEvent).Payload);
            Assert.Equal(PersonDetailsUpdatedEventChanges.DateOfBirth, personDetailsUpdatedEvent.Changes);
            Assert.Equal(newDateOfBirth, personDetailsUpdatedEvent.PersonDetails.DateOfBirth);
            Assert.Equal(person.DateOfBirth, personDetailsUpdatedEvent.OldPersonDetails.DateOfBirth);

            var emailSentEvent = Assert.IsType<EmailSentEvent>(
                Assert.Single(processEvents, pe => pe.Payload is EmailSentEvent).Payload);
            Assert.Equal(EmailTemplateIds.GetAnIdentityChangeOfDateOfBirthApprovedEmailConfirmation, emailSentEvent.Email.TemplateId);
            Assert.Equal(requestEmailAddress, emailSentEvent.Email.EmailAddress);
        });
    }

    [Fact]
    public async Task Execute_LegacyEventWithoutRequestEmailAddress_AssumesEmailWasSentToTheAddressOnTheRecord()
    {
        // Arrange
        var recordEmailAddress = TestData.GenerateUniqueEmail();
        var person = await TestData.CreatePersonAsync();

        var dbSupportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            person.PersonId,
            b => b
                .WithLastName(TestData.GenerateChangedLastName(person.LastName))
                .WithoutEmailAddress());

        var oldSupportTask = EventModels.SupportTask.FromModel(dbSupportTask);
        var supportTask = oldSupportTask with
        {
            Status = SupportTaskStatus.Closed,
            Outcome = SupportTaskOutcome.ChangeNameRequest_Approved
        };

        var requestData = dbSupportTask.GetData<ChangeNameRequestData>();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.ChangeNameRequestSupportTaskApprovedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            RequestData = EventModels.ChangeNameRequestData.FromModel(requestData),
            Changes = LegacyEvents.ChangeNameRequestSupportTaskApprovedEventChanges.LastName,
            PersonAttributes = CreatePersonDetails(person.FirstName, person.MiddleName, requestData.LastName, person.DateOfBirth, recordEmailAddress),
            OldPersonAttributes = CreatePersonDetails(person.FirstName, person.MiddleName, person.LastName, person.DateOfBirth, recordEmailAddress),
            SupportTask = supportTask,
            OldSupportTask = oldSupportTask
        });

        // Act
        await WithServiceAsync<BackfillChangeRequestApprovalProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var supportTaskUpdatedProcessEvent = await dbContext.ProcessEvents
                .SingleAsync(pe => pe.ProcessEventId == legacyEvent.EventId);

            var processEvents = await dbContext.ProcessEvents
                .Where(pe => pe.ProcessId == supportTaskUpdatedProcessEvent.ProcessId)
                .ToListAsync();

            var emailSentEvent = Assert.IsType<EmailSentEvent>(
                Assert.Single(processEvents, pe => pe.Payload is EmailSentEvent).Payload);
            Assert.Equal(recordEmailAddress, emailSentEvent.Email.EmailAddress);
        });
    }

    [Fact]
    public async Task Execute_RunTwice_DoesNotBackfillTwice()
    {
        // Arrange
        var legacyEvent = await AddApprovedNameChangeEventAsync();

        await WithServiceAsync<BackfillChangeRequestApprovalProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Act
        await WithServiceAsync<BackfillChangeRequestApprovalProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleAsync(pe => pe.ProcessEventId == legacyEvent.EventId);

            var processes = await dbContext.Processes
                .Where(p => p.SupportTaskReferences.Contains(legacyEvent.SupportTask.SupportTaskReference))
                .ToListAsync();
            Assert.Equal(processEvent.ProcessId, Assert.Single(processes).ProcessId);
        });
    }

    [Fact]
    public async Task Execute_DryRun_DoesNotCommitChanges()
    {
        // Arrange
        var legacyEvent = await AddApprovedNameChangeEventAsync();

        // Act
        await WithServiceAsync<BackfillChangeRequestApprovalProcessesJob>(
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
        await WithServiceAsync<BackfillChangeRequestApprovalProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    private async Task<LegacyEvents.ChangeNameRequestSupportTaskApprovedEvent> AddApprovedNameChangeEventAsync()
    {
        var person = await TestData.CreatePersonAsync();
        var newLastName = TestData.GenerateChangedLastName(person.LastName);

        var dbSupportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            person.PersonId,
            b => b.WithLastName(newLastName));

        var oldSupportTask = EventModels.SupportTask.FromModel(dbSupportTask);

        return await AddLegacyEventAsync(new LegacyEvents.ChangeNameRequestSupportTaskApprovedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            RequestData = EventModels.ChangeNameRequestData.FromModel(dbSupportTask.GetData<ChangeNameRequestData>()),
            Changes = LegacyEvents.ChangeNameRequestSupportTaskApprovedEventChanges.LastName,
            PersonAttributes = CreatePersonDetails(person.FirstName, person.MiddleName, newLastName, person.DateOfBirth),
            OldPersonAttributes = CreatePersonDetails(person.FirstName, person.MiddleName, person.LastName, person.DateOfBirth),
            SupportTask = oldSupportTask with
            {
                Status = SupportTaskStatus.Closed,
                Outcome = SupportTaskOutcome.ChangeNameRequest_Approved
            },
            OldSupportTask = oldSupportTask
        });
    }

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

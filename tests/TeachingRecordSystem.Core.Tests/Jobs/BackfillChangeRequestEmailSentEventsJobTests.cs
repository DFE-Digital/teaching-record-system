using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillChangeRequestEmailSentEventsJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_ApprovingProcessWithoutEmailSentEvent_LinksToTheEmailThatWasSent()
    {
        // Arrange
        var emailAddress = TestData.GenerateUniqueEmail();
        var person = await TestData.CreatePersonAsync();
        var newFirstName = TestData.GenerateChangedFirstName(person.FirstName);

        var process = await CreateChangeNameRequestProcessAsync(
            person,
            ProcessType.ChangeOfNameRequestApproving,
            emailAddress,
            rejectionReason: null,
            newFirstName);

        var existingEmail = await AddEmailAsync(
            EmailTemplateIds.GetAnIdentityChangeOfNameApprovedEmailConfirmation,
            emailAddress,
            sentOn: process.CreatedOn.AddMinutes(1));

        // Act
        await WithServiceAsync<BackfillChangeRequestEmailSentEventsJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var emailSentEvent = await GetEmailSentEventAsync(dbContext, process.ProcessId);
            Assert.Equal(existingEmail.EmailId, emailSentEvent.Email.EmailId);
            Assert.Equal(person.PersonId, emailSentEvent.PersonId);

            var emails = await dbContext.Emails.Where(e => e.EmailAddress == emailAddress).ToListAsync();
            Assert.Equal(existingEmail.EmailId, Assert.Single(emails).EmailId);
        });
    }

    [Fact]
    public async Task Execute_ApprovingProcessWithNoEmailToMatch_AddsOneAddressedByTheNewName()
    {
        // Arrange
        var emailAddress = TestData.GenerateUniqueEmail();
        var person = await TestData.CreatePersonAsync();
        var newFirstName = TestData.GenerateChangedFirstName(person.FirstName);

        var process = await CreateChangeNameRequestProcessAsync(
            person,
            ProcessType.ChangeOfNameRequestApproving,
            emailAddress,
            rejectionReason: null,
            newFirstName);

        // Act
        await WithServiceAsync<BackfillChangeRequestEmailSentEventsJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var emailSentEvent = await GetEmailSentEventAsync(dbContext, process.ProcessId);

            var email = await dbContext.Emails.SingleAsync(e => e.EmailId == emailSentEvent.Email.EmailId);
            Assert.Equal(EmailTemplateIds.GetAnIdentityChangeOfNameApprovedEmailConfirmation, email.TemplateId);
            Assert.Equal(emailAddress, email.EmailAddress);
            Assert.Equal(process.CreatedOn, email.SentOn);

            // The name change is applied before the email goes out.
            Assert.Equal(newFirstName, email.Personalization[ChangeRequestEmailConstants.FirstNameEmailPersonalisationKey]);
            Assert.DoesNotContain(ChangeRequestEmailConstants.RejectionReasonEmailPersonalisationKey, email.Personalization);
        });
    }

    [Fact]
    public async Task Execute_RejectingProcessWithNoEmailToMatch_AddsOneWithTheRejectionReasonWording()
    {
        // Arrange
        var emailAddress = TestData.GenerateUniqueEmail();
        var person = await TestData.CreatePersonAsync();

        var process = await CreateChangeNameRequestProcessAsync(
            person,
            ProcessType.ChangeOfNameRequestRejecting,
            emailAddress,
            rejectionReason: ChangeRequestRejectReason.WrongTypeOfDocument.GetDisplayName(),
            newFirstName: null);

        // Act
        await WithServiceAsync<BackfillChangeRequestEmailSentEventsJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var emailSentEvent = await GetEmailSentEventAsync(dbContext, process.ProcessId);

            var email = await dbContext.Emails.SingleAsync(e => e.EmailId == emailSentEvent.Email.EmailId);
            Assert.Equal(EmailTemplateIds.GetAnIdentityChangeOfNameRejectedEmailConfirmation, email.TemplateId);
            Assert.Equal(person.FirstName, email.Personalization[ChangeRequestEmailConstants.FirstNameEmailPersonalisationKey]);
            Assert.Equal(
                "This is because you provided the wrong type of document.",
                email.Personalization[ChangeRequestEmailConstants.RejectionReasonEmailPersonalisationKey]);
        });
    }

    [Fact]
    public async Task Execute_ProcessWithNoEmailAddressAnywhere_IsSkipped()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var process = await CreateChangeNameRequestProcessAsync(
            person,
            ProcessType.ChangeOfNameRequestRejecting,
            requestEmailAddress: null,
            rejectionReason: ChangeRequestRejectReason.ImageQuality.GetDisplayName(),
            newFirstName: null);

        // Act
        await WithServiceAsync<BackfillChangeRequestEmailSentEventsJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvents = await dbContext.ProcessEvents.Where(pe => pe.ProcessId == process.ProcessId).ToListAsync();
            Assert.DoesNotContain(processEvents, pe => pe.Payload is EmailSentEvent);
        });
    }

    [Fact]
    public async Task Execute_CancellingProcess_IsNotBackfilled()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var process = await CreateChangeNameRequestProcessAsync(
            person,
            ProcessType.ChangeOfNameRequestCancelling,
            TestData.GenerateUniqueEmail(),
            rejectionReason: null,
            newFirstName: null);

        // Act
        await WithServiceAsync<BackfillChangeRequestEmailSentEventsJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvents = await dbContext.ProcessEvents.Where(pe => pe.ProcessId == process.ProcessId).ToListAsync();
            Assert.DoesNotContain(processEvents, pe => pe.Payload is EmailSentEvent);
        });
    }

    [Fact]
    public async Task Execute_RunTwice_DoesNotAddTheEventTwice()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var process = await CreateChangeNameRequestProcessAsync(
            person,
            ProcessType.ChangeOfNameRequestRejecting,
            TestData.GenerateUniqueEmail(),
            rejectionReason: ChangeRequestRejectReason.RequestAndProofDontMatch.GetDisplayName(),
            newFirstName: null);

        await WithServiceAsync<BackfillChangeRequestEmailSentEventsJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Act
        await WithServiceAsync<BackfillChangeRequestEmailSentEventsJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvents = await dbContext.ProcessEvents.Where(pe => pe.ProcessId == process.ProcessId).ToListAsync();
            Assert.Single(processEvents, pe => pe.Payload is EmailSentEvent);
        });
    }

    [Fact]
    public async Task Execute_DryRun_DoesNotCommitChanges()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var process = await CreateChangeNameRequestProcessAsync(
            person,
            ProcessType.ChangeOfNameRequestRejecting,
            TestData.GenerateUniqueEmail(),
            rejectionReason: ChangeRequestRejectReason.RequestAndProofDontMatch.GetDisplayName(),
            newFirstName: null);

        // Act
        await WithServiceAsync<BackfillChangeRequestEmailSentEventsJob>(
            job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvents = await dbContext.ProcessEvents.Where(pe => pe.ProcessId == process.ProcessId).ToListAsync();
            Assert.DoesNotContain(processEvents, pe => pe.Payload is EmailSentEvent);
        });
    }

    private async Task<Process> CreateChangeNameRequestProcessAsync(
        Person person,
        ProcessType processType,
        string? requestEmailAddress,
        string? rejectionReason,
        string? newFirstName)
    {
        var dbSupportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            person.PersonId,
            b =>
            {
                b.WithLastName(TestData.GenerateChangedLastName(person.LastName));

                if (requestEmailAddress is not null)
                {
                    b.WithEmailAddress(requestEmailAddress);
                }
                else
                {
                    b.WithoutEmailAddress();
                }
            });

        var oldSupportTask = EventModels.SupportTask.FromModel(dbSupportTask);

        var supportTaskUpdatedEvent = new SupportTaskUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            SupportTaskReference = oldSupportTask.SupportTaskReference,
            Changes = SupportTaskUpdatedEventChanges.Status | SupportTaskUpdatedEventChanges.Data,
            SupportTask = oldSupportTask with { Status = SupportTaskStatus.Closed },
            OldSupportTask = oldSupportTask,
            Comments = null,
            RejectionReason = rejectionReason
        };

        if (newFirstName is null)
        {
            return await TestData.CreateProcessAsync(processType, userId: null, changeReason: null, supportTaskUpdatedEvent);
        }

        var personDetailsUpdatedEvent = new PersonDetailsUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            PersonDetails = CreatePersonDetails(newFirstName, person.LastName),
            OldPersonDetails = CreatePersonDetails(person.FirstName, person.LastName),
            Changes = PersonDetailsUpdatedEventChanges.NameChange
        };

        return await TestData.CreateProcessAsync(
            processType,
            userId: null,
            changeReason: null,
            supportTaskUpdatedEvent,
            personDetailsUpdatedEvent);
    }

    private static EventModels.PersonDetails CreatePersonDetails(string firstName, string lastName) => new()
    {
        FirstName = firstName,
        MiddleName = string.Empty,
        LastName = lastName,
        DateOfBirth = null,
        EmailAddress = null,
        NationalInsuranceNumber = null,
        Gender = null
    };

    private static async Task<EmailSentEvent> GetEmailSentEventAsync(TrsDbContext dbContext, Guid processId)
    {
        var processEvents = await dbContext.ProcessEvents.Where(pe => pe.ProcessId == processId).ToListAsync();
        return Assert.IsType<EmailSentEvent>(Assert.Single(processEvents, pe => pe.Payload is EmailSentEvent).Payload);
    }

    private async Task<Email> AddEmailAsync(string templateId, string emailAddress, DateTime sentOn)
    {
        var email = new Email
        {
            EmailId = Guid.NewGuid(),
            TemplateId = templateId,
            EmailAddress = emailAddress,
            Personalization = new Dictionary<string, string>(),
            SentOn = sentOn
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Emails.Add(email);
            await dbContext.SaveChangesAsync();
        });

        return email;
    }
}

using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class RepairChangeRequestRejectionEmailsJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    // The rejection template the Support UI used before #2661 swapped it for one with a reason field.
    private const string RetiredChangeOfNameRejectedEmailConfirmation = "bc790721-11c7-42e0-8f88-41ea96296602";

    private static readonly DateTimeOffset _beforeRejectionTemplatesChanged = new(2025, 10, 1, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _afterRejectionTemplatesChanged = new(2025, 11, 1, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Execute_FabricatedEmail_RepointsTheEventAtTheRealOneAndDeletesTheFabricatedRow()
    {
        // Arrange
        TimeProvider.SetUtcNow(_beforeRejectionTemplatesChanged);

        var emailAddress = TestData.GenerateUniqueEmail();
        var person = await TestData.CreatePersonAsync();

        var realEmail = await AddEmailAsync(
            RetiredChangeOfNameRejectedEmailConfirmation,
            emailAddress,
            sentOn: TimeProvider.UtcNow.AddMinutes(1));

        var fabricatedEmail = await AddEmailAsync(
            EmailTemplateIds.GetAnIdentityChangeOfNameRejectedEmailConfirmation,
            emailAddress,
            sentOn: TimeProvider.UtcNow);

        var process = await CreateRejectingProcessWithEmailSentEventAsync(person, fabricatedEmail);

        // Act
        await WithServiceAsync<RepairChangeRequestRejectionEmailsJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var emailSentEvent = await GetEmailSentEventAsync(dbContext, process.ProcessId);
            Assert.Equal(realEmail.EmailId, emailSentEvent.Email.EmailId);
            Assert.Equal(RetiredChangeOfNameRejectedEmailConfirmation, emailSentEvent.Email.TemplateId);

            var remaining = await dbContext.Emails.Where(e => e.EmailAddress == emailAddress).ToListAsync();
            Assert.Equal(realEmail.EmailId, Assert.Single(remaining).EmailId);
        });
    }

    [Fact]
    public async Task Execute_WithNoRealEmailToMatch_LeavesEverythingAlone()
    {
        // Arrange
        TimeProvider.SetUtcNow(_beforeRejectionTemplatesChanged);

        var emailAddress = TestData.GenerateUniqueEmail();
        var person = await TestData.CreatePersonAsync();

        var fabricatedEmail = await AddEmailAsync(
            EmailTemplateIds.GetAnIdentityChangeOfNameRejectedEmailConfirmation,
            emailAddress,
            sentOn: TimeProvider.UtcNow);

        var process = await CreateRejectingProcessWithEmailSentEventAsync(person, fabricatedEmail);

        // Act
        await WithServiceAsync<RepairChangeRequestRejectionEmailsJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            // Deleting a row the event still points at would be worse than leaving it wrong.
            var emailSentEvent = await GetEmailSentEventAsync(dbContext, process.ProcessId);
            Assert.Equal(fabricatedEmail.EmailId, emailSentEvent.Email.EmailId);
            Assert.NotNull(await dbContext.Emails.SingleOrDefaultAsync(e => e.EmailId == fabricatedEmail.EmailId));
        });
    }

    [Fact]
    public async Task Execute_RejectionFromAfterTheTemplatesChanged_IsLeftAlone()
    {
        // Arrange
        TimeProvider.SetUtcNow(_afterRejectionTemplatesChanged);

        var emailAddress = TestData.GenerateUniqueEmail();
        var person = await TestData.CreatePersonAsync();

        var email = await AddEmailAsync(
            EmailTemplateIds.GetAnIdentityChangeOfNameRejectedEmailConfirmation,
            emailAddress,
            sentOn: TimeProvider.UtcNow);

        var process = await CreateRejectingProcessWithEmailSentEventAsync(person, email);

        // Act
        await WithServiceAsync<RepairChangeRequestRejectionEmailsJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var emailSentEvent = await GetEmailSentEventAsync(dbContext, process.ProcessId);
            Assert.Equal(email.EmailId, emailSentEvent.Email.EmailId);
            Assert.NotNull(await dbContext.Emails.SingleOrDefaultAsync(e => e.EmailId == email.EmailId));
        });
    }

    [Fact]
    public async Task Execute_RunTwice_IsIdempotent()
    {
        // Arrange
        TimeProvider.SetUtcNow(_beforeRejectionTemplatesChanged);

        var emailAddress = TestData.GenerateUniqueEmail();
        var person = await TestData.CreatePersonAsync();

        var realEmail = await AddEmailAsync(
            RetiredChangeOfNameRejectedEmailConfirmation,
            emailAddress,
            sentOn: TimeProvider.UtcNow.AddMinutes(1));

        var fabricatedEmail = await AddEmailAsync(
            EmailTemplateIds.GetAnIdentityChangeOfNameRejectedEmailConfirmation,
            emailAddress,
            sentOn: TimeProvider.UtcNow);

        var process = await CreateRejectingProcessWithEmailSentEventAsync(person, fabricatedEmail);

        await WithServiceAsync<RepairChangeRequestRejectionEmailsJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Act
        await WithServiceAsync<RepairChangeRequestRejectionEmailsJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var emailSentEvent = await GetEmailSentEventAsync(dbContext, process.ProcessId);
            Assert.Equal(realEmail.EmailId, emailSentEvent.Email.EmailId);
            Assert.Single(await dbContext.Emails.Where(e => e.EmailAddress == emailAddress).ToListAsync());
        });
    }

    private async Task<Process> CreateRejectingProcessWithEmailSentEventAsync(Person person, Email email)
    {
        var dbSupportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            person.PersonId,
            b => b.WithLastName(TestData.GenerateChangedLastName(person.LastName)));

        var oldSupportTask = EventModels.SupportTask.FromModel(dbSupportTask);

        return await TestData.CreateProcessAsync(
            ProcessType.ChangeOfNameRequestRejecting,
            userId: null,
            changeReason: null,
            new SupportTaskUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTaskReference = oldSupportTask.SupportTaskReference,
                Changes = SupportTaskUpdatedEventChanges.Status | SupportTaskUpdatedEventChanges.Data,
                SupportTask = oldSupportTask with { Status = SupportTaskStatus.Closed },
                OldSupportTask = oldSupportTask,
                Comments = null,
                RejectionReason = ChangeRequestRejectReason.WrongTypeOfDocument.GetDisplayName()
            },
            new EmailSentEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Email = EventModels.Email.FromModel(email)
            });
    }

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

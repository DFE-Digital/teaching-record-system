using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillOverseasNpqTrnEmailSentEventsJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    private const string TrnPersonalizationKey = "trn";

    [Fact]
    public async Task Execute_OrphanedEmail_AttachesEventToThePersonCreationProcess()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var process = await CreatePersonCreationProcessAsync(person);
        var email = await AddEmailAsync(EmailTemplateIds.TrnGeneratedForNpq, person.Trn!);
        var legacyEvent = await AddLegacyEmailSentEventAsync(email);

        // Act
        await WithServiceAsync<BackfillOverseasNpqTrnEmailSentEventsJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents
                .SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(process.ProcessId, processEvent.ProcessId);
            Assert.Equal(nameof(EmailSentEvent), processEvent.EventName);
            Assert.Equal(email.SentOn, processEvent.CreatedOn);

            var emailSentEvent = Assert.IsType<EmailSentEvent>(processEvent.Payload);
            Assert.Equal(person.PersonId, emailSentEvent.PersonId);
            Assert.Equal(email.EmailId, emailSentEvent.Email.EmailId);
            Assert.Equal(EmailTemplateIds.TrnGeneratedForNpq, emailSentEvent.Email.TemplateId);
            Assert.Equal(person.Trn, emailSentEvent.Email.Personalization[TrnPersonalizationKey]);
        });
    }

    [Fact]
    public async Task Execute_EmailAlreadyAttachedToAProcess_IsLeftAlone()
    {
        // Arrange
        // The NPQ TRN request journey sends the same template with a process id, so its email already has an
        // event. The legacy row it wrote alongside carries a different id, so only the email links the two.
        var person = await TestData.CreatePersonAsync();
        await CreatePersonCreationProcessAsync(person);
        var email = await AddEmailAsync(EmailTemplateIds.TrnGeneratedForNpq, person.Trn!);

        var existingProcess = await TestData.CreateProcessAsync(
            ProcessType.NpqTrnRequestApproving,
            userId: null,
            changeReason: null,
            new EmailSentEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Email = EventModels.Email.FromModel(email)
            });

        await AddLegacyEmailSentEventAsync(email);

        // Act
        await WithServiceAsync<BackfillOverseasNpqTrnEmailSentEventsJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvents = await dbContext.ProcessEvents
                .Where(pe => pe.EventName == nameof(EmailSentEvent) && pe.PersonIds.Contains(person.PersonId))
                .ToListAsync();
            Assert.Equal(existingProcess.ProcessId, Assert.Single(processEvents).ProcessId);
        });
    }

    [Fact]
    public async Task Execute_RunTwice_DoesNotAddDuplicateEvents()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await CreatePersonCreationProcessAsync(person);
        var email = await AddEmailAsync(EmailTemplateIds.TrnGeneratedForNpq, person.Trn!);
        await AddLegacyEmailSentEventAsync(email);

        await WithServiceAsync<BackfillOverseasNpqTrnEmailSentEventsJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Act
        await WithServiceAsync<BackfillOverseasNpqTrnEmailSentEventsJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvents = await dbContext.ProcessEvents
                .Where(pe => pe.EventName == nameof(EmailSentEvent) && pe.PersonIds.Contains(person.PersonId))
                .ToListAsync();
            Assert.Single(processEvents);
        });
    }

    [Fact]
    public async Task Execute_TrnDoesNotMatchAPerson_IsLeftAlone()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await CreatePersonCreationProcessAsync(person);
        var email = await AddEmailAsync(EmailTemplateIds.TrnGeneratedForNpq, trn: "9999999");
        var legacyEvent = await AddLegacyEmailSentEventAsync(email);

        // Act
        await WithServiceAsync<BackfillOverseasNpqTrnEmailSentEventsJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents
                .SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    [Fact]
    public async Task Execute_PersonHasNoCreationProcess_IsLeftAlone()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var email = await AddEmailAsync(EmailTemplateIds.TrnGeneratedForNpq, person.Trn!);
        var legacyEvent = await AddLegacyEmailSentEventAsync(email);

        // Act
        await WithServiceAsync<BackfillOverseasNpqTrnEmailSentEventsJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents
                .SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    [Fact]
    public async Task Execute_PersonHasMoreThanOneCreationProcess_IsLeftAlone()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await CreatePersonCreationProcessAsync(person);
        await CreatePersonCreationProcessAsync(person);
        var email = await AddEmailAsync(EmailTemplateIds.TrnGeneratedForNpq, person.Trn!);
        var legacyEvent = await AddLegacyEmailSentEventAsync(email);

        // Act
        await WithServiceAsync<BackfillOverseasNpqTrnEmailSentEventsJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents
                .SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    [Fact]
    public async Task Execute_EmailOnAnotherTemplate_IsLeftAlone()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await CreatePersonCreationProcessAsync(person);
        var email = await AddEmailAsync(EmailTemplateIds.QtsAwardedEmailConfirmation, person.Trn!);
        var legacyEvent = await AddLegacyEmailSentEventAsync(email);

        // Act
        await WithServiceAsync<BackfillOverseasNpqTrnEmailSentEventsJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents
                .SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    [Fact]
    public async Task Execute_EmailRowHasBeenDeleted_IsLeftAlone()
    {
        // Arrange
        // An event pointing at a row that no longer exists is worse than no event at all.
        var person = await TestData.CreatePersonAsync();
        await CreatePersonCreationProcessAsync(person);
        var email = await AddEmailAsync(EmailTemplateIds.TrnGeneratedForNpq, person.Trn!);
        var legacyEvent = await AddLegacyEmailSentEventAsync(email);

        await WithDbContextAsync(async dbContext =>
        {
            await dbContext.Emails.Where(e => e.EmailId == email.EmailId).ExecuteDeleteAsync();
        });

        // Act
        await WithServiceAsync<BackfillOverseasNpqTrnEmailSentEventsJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents
                .SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    private Task<Process> CreatePersonCreationProcessAsync(Person person) =>
        TestData.CreateProcessAsync(
            ProcessType.PersonCreating,
            userId: null,
            changeReason: null,
            new PersonCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Details = EventModels.PersonDetails.FromModel(person),
                TrnRequestMetadata = null
            });

    private async Task<Email> AddEmailAsync(string templateId, string trn)
    {
        var email = new Email
        {
            EmailId = Guid.NewGuid(),
            TemplateId = templateId,
            EmailAddress = TestData.GenerateUniqueEmail(),
            Personalization = new Dictionary<string, string> { { TrnPersonalizationKey, trn } },
            SentOn = TimeProvider.UtcNow
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Emails.Add(email);
            await dbContext.SaveChangesAsync();
        });

        return email;
    }

    private async Task<LegacyEvents.EmailSentEvent> AddLegacyEmailSentEventAsync(Email email)
    {
        var legacyEvent = new LegacyEvents.EmailSentEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            Email = EventModels.Email.FromModel(email)
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(legacyEvent);
            await dbContext.SaveChangesAsync();
        });

        return legacyEvent;
    }
}

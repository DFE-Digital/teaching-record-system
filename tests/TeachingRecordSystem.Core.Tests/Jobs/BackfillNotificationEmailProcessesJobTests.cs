using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillNotificationEmailProcessesJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    private static readonly ProcessType[] _notificationProcessTypes =
    [
        ProcessType.NotifyingQtsAwardee,
        ProcessType.NotifyingInternationalQtsAwardee,
        ProcessType.NotifyingEytsAwardee,
        ProcessType.NotifyingQtlsAwardee,
        ProcessType.NotifyingInductionCompletee,
        ProcessType.NotifyingLapsedQtlsHolder
    ];

    [Fact]
    public async Task Execute_LegacyQtsAwardedEmailSentEvent_CreatesProcessWithEmailSentEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var emailAddress = Faker.Internet.Email();
        var personalization = new Dictionary<string, string> { { "first name", person.FirstName } };

        var jobId = Guid.NewGuid();
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.QtsAwardedEmailsJobs.Add(new QtsAwardedEmailsJob
            {
                QtsAwardedEmailsJobId = jobId,
                AwardedToUtc = TimeProvider.UtcNow.AddDays(-1),
                ExecutedUtc = TimeProvider.UtcNow
            });

            dbContext.QtsAwardedEmailsJobItems.Add(new QtsAwardedEmailsJobItem
            {
                QtsAwardedEmailsJobId = jobId,
                PersonId = person.PersonId,
                Trn = person.Trn!,
                EmailAddress = emailAddress,
                Personalization = personalization,
                EmailSent = true
            });

            await dbContext.SaveChangesAsync();
        });

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.QtsAwardedEmailSentEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            QtsAwardedEmailsJobId = jobId,
            PersonId = person.PersonId,
            EmailAddress = emailAddress
        });

        // Act
        await WithServiceAsync<BackfillNotificationEmailProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents
                .SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(EmailSentEvent), processEvent.EventName);

            var emailSentEvent = Assert.IsType<EmailSentEvent>(processEvent.Payload);
            Assert.Equal(person.PersonId, emailSentEvent.PersonId);
            Assert.Equal(EmailTemplateIds.QtsAwardedEmailConfirmation, emailSentEvent.Email.TemplateId);
            Assert.Equal(emailAddress, emailSentEvent.Email.EmailAddress);
            Assert.Equal(personalization, emailSentEvent.Email.Personalization);
            Assert.Equal(person.Trn, Assert.Contains(SendAytqInviteEmailJob.JobMetadataKeys.Trn, emailSentEvent.Email.Metadata).ToString());
            Assert.Equal(legacyEvent.CreatedUtc, emailSentEvent.Email.SentOn);

            var email = await dbContext.Emails.SingleOrDefaultAsync(e => e.EmailId == emailSentEvent.Email.EmailId);
            Assert.NotNull(email);
            Assert.Equal(legacyEvent.CreatedUtc, email.SentOn);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.NotifyingQtsAwardee, process.ProcessType);
            Assert.Equal(SystemUser.SystemUserId, process.UserId);
            Assert.Equal(legacyEvent.CreatedUtc, process.CreatedOn);
            Assert.Equal(person.PersonId, Assert.Single(process.PersonIds));
            Assert.Null(process.ChangeReason);
        });
    }

    [Fact]
    public async Task Execute_LegacyInternationalQtsAwardedEmailSentEvent_CreatesProcessWithEmailSentEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var emailAddress = Faker.Internet.Email();

        var jobId = Guid.NewGuid();
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.InternationalQtsAwardedEmailsJobs.Add(new InternationalQtsAwardedEmailsJob
            {
                InternationalQtsAwardedEmailsJobId = jobId,
                AwardedToUtc = TimeProvider.UtcNow.AddDays(-1),
                ExecutedUtc = TimeProvider.UtcNow
            });

            dbContext.InternationalQtsAwardedEmailsJobItems.Add(new InternationalQtsAwardedEmailsJobItem
            {
                InternationalQtsAwardedEmailsJobId = jobId,
                PersonId = person.PersonId,
                Trn = person.Trn!,
                EmailAddress = emailAddress,
                Personalization = new Dictionary<string, string> { { "first name", person.FirstName } },
                EmailSent = true
            });

            await dbContext.SaveChangesAsync();
        });

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.InternationalQtsAwardedEmailSentEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            InternationalQtsAwardedEmailsJobId = jobId,
            PersonId = person.PersonId,
            EmailAddress = emailAddress
        });

        // Act
        await WithServiceAsync<BackfillNotificationEmailProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents
                .SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);

            var emailSentEvent = Assert.IsType<EmailSentEvent>(processEvent.Payload);
            Assert.Equal(EmailTemplateIds.InternationalQtsAwardedEmailConfirmation, emailSentEvent.Email.TemplateId);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.NotifyingInternationalQtsAwardee, process.ProcessType);
        });
    }

    [Fact]
    public async Task Execute_LegacyEytsAwardedEmailSentEvent_CreatesProcessWithEmailSentEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var emailAddress = Faker.Internet.Email();

        var jobId = Guid.NewGuid();
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.EytsAwardedEmailsJobs.Add(new EytsAwardedEmailsJob
            {
                EytsAwardedEmailsJobId = jobId,
                AwardedToUtc = TimeProvider.UtcNow.AddDays(-1),
                ExecutedUtc = TimeProvider.UtcNow
            });

            dbContext.EytsAwardedEmailsJobItems.Add(new EytsAwardedEmailsJobItem
            {
                EytsAwardedEmailsJobId = jobId,
                PersonId = person.PersonId,
                Trn = person.Trn!,
                EmailAddress = emailAddress,
                Personalization = new Dictionary<string, string> { { "first name", person.FirstName } },
                EmailSent = true
            });

            await dbContext.SaveChangesAsync();
        });

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.EytsAwardedEmailSentEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            EytsAwardedEmailsJobId = jobId,
            PersonId = person.PersonId,
            EmailAddress = emailAddress
        });

        // Act
        await WithServiceAsync<BackfillNotificationEmailProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents
                .SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);

            var emailSentEvent = Assert.IsType<EmailSentEvent>(processEvent.Payload);
            Assert.Equal(EmailTemplateIds.EytsAwardedEmailConfirmation, emailSentEvent.Email.TemplateId);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.NotifyingEytsAwardee, process.ProcessType);
        });
    }

    [Fact]
    public async Task Execute_LegacyInductionCompletedEmailSentEvent_CreatesProcessWithEmailSentEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var emailAddress = Faker.Internet.Email();

        var jobId = Guid.NewGuid();
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.InductionCompletedEmailsJobs.Add(new InductionCompletedEmailsJob
            {
                InductionCompletedEmailsJobId = jobId,
                PassedEndUtc = TimeProvider.UtcNow.AddDays(-1),
                ExecutedUtc = TimeProvider.UtcNow
            });

            dbContext.InductionCompletedEmailsJobItems.Add(new InductionCompletedEmailsJobItem
            {
                InductionCompletedEmailsJobId = jobId,
                PersonId = person.PersonId,
                Trn = person.Trn!,
                EmailAddress = emailAddress,
                Personalization = new Dictionary<string, string> { { "first name", person.FirstName } },
                EmailSent = true
            });

            await dbContext.SaveChangesAsync();
        });

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.InductionCompletedEmailSentEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            InductionCompletedEmailsJobId = jobId,
            PersonId = person.PersonId,
            EmailAddress = emailAddress
        });

        // Act
        await WithServiceAsync<BackfillNotificationEmailProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents
                .SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);

            var emailSentEvent = Assert.IsType<EmailSentEvent>(processEvent.Payload);
            Assert.Equal(EmailTemplateIds.InductionCompletedEmailConfirmation, emailSentEvent.Email.TemplateId);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.NotifyingInductionCompletee, process.ProcessType);
        });
    }

    [Fact]
    public async Task Execute_LegacyEventWithNoJobItem_CreatesEmailWithoutPersonalization()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var emailAddress = Faker.Internet.Email();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.QtsAwardedEmailSentEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            QtsAwardedEmailsJobId = Guid.NewGuid(),
            PersonId = person.PersonId,
            EmailAddress = emailAddress
        });

        // Act
        await WithServiceAsync<BackfillNotificationEmailProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents
                .SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);

            var emailSentEvent = Assert.IsType<EmailSentEvent>(processEvent.Payload);
            Assert.Equal(emailAddress, emailSentEvent.Email.EmailAddress);
            Assert.Empty(emailSentEvent.Email.Personalization);
            Assert.Empty(emailSentEvent.Email.Metadata);
        });
    }

    [Fact]
    public async Task Execute_RunTwice_DoesNotCreateDuplicateProcesses()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        await AddLegacyEventAsync(new LegacyEvents.QtsAwardedEmailSentEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            QtsAwardedEmailsJobId = Guid.NewGuid(),
            PersonId = person.PersonId,
            EmailAddress = Faker.Internet.Email()
        });

        await WithServiceAsync<BackfillNotificationEmailProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Act
        await WithServiceAsync<BackfillNotificationEmailProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processes = await dbContext.Processes
                .Where(p => p.PersonIds.Contains(person.PersonId) && p.ProcessType == ProcessType.NotifyingQtsAwardee)
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
        await WithServiceAsync<BackfillNotificationEmailProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processes = await dbContext.Processes
                .Where(p => p.PersonIds.Contains(person.PersonId) && _notificationProcessTypes.Contains(p.ProcessType))
                .ToListAsync();
            Assert.Empty(processes);
        });
    }

    [Theory]
    [InlineData(EmailTemplateIds.QtsAwardedEmailConfirmation, ProcessType.NotifyingQtsAwardee)]
    [InlineData(EmailTemplateIds.InternationalQtsAwardedEmailConfirmation, ProcessType.NotifyingInternationalQtsAwardee)]
    [InlineData(EmailTemplateIds.EytsAwardedEmailConfirmation, ProcessType.NotifyingEytsAwardee)]
    [InlineData(EmailTemplateIds.QtlsPostLaunchForAllUsers, ProcessType.NotifyingQtlsAwardee)]
    public async Task Execute_LegacyEmailSentEventWithTrn_CreatesProcessPointingAtTheExistingEmail(
        string templateId,
        ProcessType expectedProcessType)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithEmailAddress(TestData.GenerateUniqueEmail()));
        var email = await CreateSentEmailAsync(
            templateId,
            person.EmailAddress!,
            new Dictionary<string, object> { [SendAytqInviteEmailJob.JobMetadataKeys.Trn] = person.Trn! });

        var legacyEvent = await AddLegacyEmailSentEventAsync(email);

        // Act
        await WithServiceAsync<BackfillNotificationEmailProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents
                .SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);

            var emailSentEvent = Assert.IsType<EmailSentEvent>(processEvent.Payload);
            Assert.Equal(person.PersonId, emailSentEvent.PersonId);

            // The send created a real email row, so the event points at it rather than a new one.
            Assert.Equal(email.EmailId, emailSentEvent.Email.EmailId);
            Assert.Single(await dbContext.Emails.Where(e => e.EmailAddress == person.EmailAddress).ToListAsync());

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(expectedProcessType, process.ProcessType);
            Assert.Equal(person.PersonId, Assert.Single(process.PersonIds));
        });
    }

    [Fact]
    public async Task Execute_LegacyEmailSentEventWithUnknownTrn_IsSkipped()
    {
        // Arrange
        var email = await CreateSentEmailAsync(
            EmailTemplateIds.QtsAwardedEmailConfirmation,
            TestData.GenerateUniqueEmail(),
            new Dictionary<string, object> { [SendAytqInviteEmailJob.JobMetadataKeys.Trn] = "9999999" });

        var legacyEvent = await AddLegacyEmailSentEventAsync(email);

        // Act
        await WithServiceAsync<BackfillNotificationEmailProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
            Assert.Null(await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId)));
    }

    [Fact]
    public async Task Execute_LegacyQtlsLapsedEmailSentEvent_ResolvesPersonFromTheQtlsExpiry()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithEmailAddress(TestData.GenerateUniqueEmail()));
        await AddQtlsExpiryAsync(person.PersonId);

        var email = await CreateSentEmailAsync(EmailTemplateIds.QtlsLapsed, person.EmailAddress!, new Dictionary<string, object>());
        var legacyEvent = await AddLegacyEmailSentEventAsync(email);

        // Act
        await WithServiceAsync<BackfillNotificationEmailProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents
                .SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);

            var emailSentEvent = Assert.IsType<EmailSentEvent>(processEvent.Payload);
            Assert.Equal(person.PersonId, emailSentEvent.PersonId);
            Assert.Equal(email.EmailId, emailSentEvent.Email.EmailId);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.NotifyingLapsedQtlsHolder, process.ProcessType);
        });
    }

    [Fact]
    public async Task Execute_LegacyQtlsLapsedEmailSentEventWithNoQtlsExpiry_IsSkipped()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithEmailAddress(TestData.GenerateUniqueEmail()));

        var email = await CreateSentEmailAsync(EmailTemplateIds.QtlsLapsed, person.EmailAddress!, new Dictionary<string, object>());
        var legacyEvent = await AddLegacyEmailSentEventAsync(email);

        // Act
        await WithServiceAsync<BackfillNotificationEmailProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
            Assert.Null(await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId)));
    }

    [Fact]
    public async Task Execute_LegacyQtlsLapsedEmailSentEventMatchingTwoPeople_IsSkipped()
    {
        // Arrange
        var emailAddress = TestData.GenerateUniqueEmail();
        var firstPerson = await TestData.CreatePersonAsync(p => p.WithEmailAddress(emailAddress));
        var secondPerson = await TestData.CreatePersonAsync(p => p.WithEmailAddress(emailAddress));
        await AddQtlsExpiryAsync(firstPerson.PersonId);
        await AddQtlsExpiryAsync(secondPerson.PersonId);

        var email = await CreateSentEmailAsync(EmailTemplateIds.QtlsLapsed, emailAddress, new Dictionary<string, object>());
        var legacyEvent = await AddLegacyEmailSentEventAsync(email);

        // Act
        await WithServiceAsync<BackfillNotificationEmailProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
            Assert.Null(await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId)));
    }

    [Fact]
    public async Task Execute_LegacyEmailSentEventForAnUnrelatedTemplate_IsLeftAlone()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithEmailAddress(TestData.GenerateUniqueEmail()));
        var email = await CreateSentEmailAsync(
            EmailTemplateIds.TraineeTrnRecipient,
            person.EmailAddress!,
            new Dictionary<string, object> { [SendAytqInviteEmailJob.JobMetadataKeys.Trn] = person.Trn! });

        var legacyEvent = await AddLegacyEmailSentEventAsync(email);

        // Act
        await WithServiceAsync<BackfillNotificationEmailProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
            Assert.Null(await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId)));
    }

    private Task<Email> CreateSentEmailAsync(string templateId, string emailAddress, Dictionary<string, object> metadata) =>
        WithDbContextAsync(async dbContext =>
        {
            var email = new Email
            {
                EmailId = Guid.NewGuid(),
                TemplateId = templateId,
                EmailAddress = emailAddress,
                Personalization = new Dictionary<string, string>(),
                Metadata = metadata,
                SentOn = TimeProvider.UtcNow
            };

            dbContext.Emails.Add(email);
            await dbContext.SaveChangesAsync();
            return email;
        });

    private Task<LegacyEvents.EmailSentEvent> AddLegacyEmailSentEventAsync(Email email) =>
        AddLegacyEventAsync(new LegacyEvents.EmailSentEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            Email = EventModels.Email.FromModel(email)
        });

    private Task AddQtlsExpiryAsync(Guid personId) =>
        TestData.CreateProcessAsync(
            ProcessType.RouteToProfessionalStatusDeleting,
            SystemUser.SystemUserId,
            changeReason: null,
            new PersonProfessionalStatusAttributesUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = personId,
                PersonAttributes = CreateProfessionalStatusAttributes(QtlsStatus.Expired),
                OldPersonAttributes = CreateProfessionalStatusAttributes(QtlsStatus.Active),
                Changes = PersonProfessionalStatusAttributesUpdatedEventChanges.QtlsStatus
            });

    private static EventModels.ProfessionalStatusPersonAttributes CreateProfessionalStatusAttributes(QtlsStatus qtlsStatus) =>
        new()
        {
            QtsDate = null,
            EytsDate = null,
            HasEyps = false,
            PqtsDate = null,
            QtlsStatus = qtlsStatus
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

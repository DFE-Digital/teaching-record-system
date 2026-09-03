using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillNotificationEmailProcessesJobTests(JobFixture fixture) : JobTestBase(fixture)
{
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
            Assert.Equal(ProcessType.NotifyingProfessionalStatusAwardee, process.ProcessType);
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
            Assert.Equal(ProcessType.NotifyingProfessionalStatusAwardee, process.ProcessType);
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
            Assert.Equal(ProcessType.NotifyingProfessionalStatusAwardee, process.ProcessType);
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
                .Where(p => p.PersonIds.Contains(person.PersonId) && p.ProcessType == ProcessType.NotifyingProfessionalStatusAwardee)
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
                .Where(p => p.PersonIds.Contains(person.PersonId) &&
                    (p.ProcessType == ProcessType.NotifyingProfessionalStatusAwardee || p.ProcessType == ProcessType.NotifyingInductionCompletee))
                .ToListAsync();
            Assert.Empty(processes);
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

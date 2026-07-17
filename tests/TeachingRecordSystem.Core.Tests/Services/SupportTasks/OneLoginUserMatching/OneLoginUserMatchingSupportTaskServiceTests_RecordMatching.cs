using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

namespace TeachingRecordSystem.Core.Tests.Services.SupportTasks.OneLoginUserMatching;

public partial class OneLoginUserMatchingSupportTaskServiceTests
{
    [Fact]
    public async Task CreateRecordMatchingSupportTaskAsync_CreatesSupportTaskWithExpectedData()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
        var verifiedNames = new[] { new[] { Faker.Name.First(), Faker.Name.Last() } };
        var verifiedDatesOfBirth = new[] { DateOnly.FromDateTime(Faker.Identification.DateOfBirth()) };
        var statedNationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
        var statedTrn = "0000000";
        var clientApplicationUserId = Guid.NewGuid();
        var trnTokenTrn = "0000000";

        var options = new CreateOneLoginUserRecordMatchingSupportTaskOptions
        {
            OneLoginUserSubject = oneLoginUser.Subject,
            OneLoginUserEmailAddress = oneLoginUser.EmailAddress!,
            VerifiedNames = verifiedNames,
            VerifiedDatesOfBirth = verifiedDatesOfBirth,
            StatedNationalInsuranceNumber = statedNationalInsuranceNumber,
            StatedTrn = statedTrn,
            ClientApplicationUserId = clientApplicationUserId,
            TrnTokenTrn = trnTokenTrn
        };

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        var supportTask = await WithServiceAsync(s => s.CreateRecordMatchingSupportTaskAsync(options, processContext));

        // Assert
        Assert.NotNull(supportTask);
        Assert.Equal(SupportTaskType.OneLoginUserRecordMatching, supportTask.SupportTaskType);
        Assert.Equal(SupportTaskStatus.Open, supportTask.Status);
        Assert.Equal(oneLoginUser.Subject, supportTask.OneLoginUserSubject);
        Assert.NotNull(supportTask.SupportTaskReference);

        var data = supportTask.GetData<OneLoginUserRecordMatchingData>();
        Assert.Equal(oneLoginUser.Subject, data.OneLoginUserSubject);
        Assert.Equal(oneLoginUser.EmailAddress, data.OneLoginUserEmail);
        Assert.Equal(verifiedNames, data.VerifiedNames);
        Assert.Equal(verifiedDatesOfBirth, data.VerifiedDatesOfBirth);
        Assert.Equal(statedNationalInsuranceNumber, data.StatedNationalInsuranceNumber);
        Assert.Equal(statedTrn, data.StatedTrn);
        Assert.Equal(clientApplicationUserId, data.ClientApplicationUserId);
        Assert.Equal(trnTokenTrn, data.TrnTokenTrn);

        Events.AssertEventsPublished(e => Assert.IsType<SupportTaskCreatedEvent>(e));
    }

    [Fact]
    public async Task ResolveRecordMatchingSupportTaskAsync_WithNotConnectingOutcome_ClosesSupportTaskSetsOutcomeAsExpected()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject);

        var data = supportTask.GetData<OneLoginUserRecordMatchingData>();

        var notConnectingReason = OneLoginUserNotConnectingReason.AnotherReason;
        var notConnectingAdditionalDetails = Faker.Lorem.Paragraph();

        var options = new NotConnectingOutcomeOptions
        {
            SupportTask = supportTask,
            NotConnectingReason = notConnectingReason,
            NotConnectingAdditionalDetails = notConnectingAdditionalDetails
        };

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ResolveRecordMatchingSupportTaskAsync(options, processContext));

        // Assert
        var updatedSupportTask =
            await WithDbContextAsync(dbContext => dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference));
        var updatedData = updatedSupportTask.GetData<OneLoginUserRecordMatchingData>();
        Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
        Assert.Equal(nameof(OneLoginUserRecordMatchingOutcome.NotConnecting), updatedSupportTask.OutcomeLabel);
        Assert.Null(updatedData.PersonId);
        Assert.Equal(OneLoginUserRecordMatchingOutcome.NotConnecting, updatedData.Outcome);
        Assert.Equal(notConnectingReason, updatedData.NotConnectingReason);
        Assert.Equal(notConnectingAdditionalDetails, updatedData.NotConnectingAdditionalDetails);

        Events.AssertEventsPublished(e => Assert.IsType<SupportTaskUpdatedEvent>(e));
    }

    [Fact]
    public async Task ResolveRecordMatchingSupportTaskAsync_WithNotConnectingOutcome_WithDeferredPolicyAndCustomEmailTemplateId_SendsEmailWithReason()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

        var customTemplateId = "custom-template-id";
        var notConnectingReason = OneLoginUserNotConnectingReason.NoMatchingRecord;

        var applicationUser = await TestData.CreateApplicationUserAsync(
            recordMatchingPolicy: RecordMatchingPolicy.Deferred,
            appContent: new AppContent { OneLoginNotConnectedEmailTemplateId = customTemplateId });

        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject, t => t.WithClientApplicationUserId(applicationUser.UserId));

        var options = new NotConnectingOutcomeOptions
        {
            SupportTask = supportTask,
            NotConnectingReason = notConnectingReason,
            NotConnectingAdditionalDetails = null
        };

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ResolveRecordMatchingSupportTaskAsync(options, processContext));

        // Assert
        await BackgroundJobScheduler.ExecuteDeferredJobsAsync();
        var emails = await WithDbContextAsync(dbContext => dbContext.Emails.Where(e => e.EmailAddress == oneLoginUser.EmailAddress).ToArrayAsync());
        Assert.Collection(emails,
            e =>
            {
                Assert.Equal(customTemplateId, e.TemplateId);
                Assert.Equal(notConnectingReason.GetDisplayName(), e.Personalization["reason"]);
            });
    }

    [Fact]
    public async Task ResolveRecordMatchingSupportTaskAsync_WithNotConnectingOutcome_WithDeferredPolicyAndAnotherReason_SendsEmailWithAdditionalDetails()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

        var customTemplateId = "custom-template-id";
        var notConnectingAdditionalDetails = Faker.Lorem.Paragraph();

        var applicationUser = await TestData.CreateApplicationUserAsync(
            recordMatchingPolicy: RecordMatchingPolicy.Deferred,
            appContent: new AppContent { OneLoginNotConnectedEmailTemplateId = customTemplateId });

        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject, t => t.WithClientApplicationUserId(applicationUser.UserId));

        var options = new NotConnectingOutcomeOptions
        {
            SupportTask = supportTask,
            NotConnectingReason = OneLoginUserNotConnectingReason.AnotherReason,
            NotConnectingAdditionalDetails = notConnectingAdditionalDetails
        };

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ResolveRecordMatchingSupportTaskAsync(options, processContext));

        // Assert
        await BackgroundJobScheduler.ExecuteDeferredJobsAsync();
        var emails = await WithDbContextAsync(dbContext => dbContext.Emails.Where(e => e.EmailAddress == oneLoginUser.EmailAddress).ToArrayAsync());
        Assert.Collection(emails,
            e =>
            {
                Assert.Equal(customTemplateId, e.TemplateId);
                Assert.Equal(notConnectingAdditionalDetails, e.Personalization["reason"]);
            });
    }

    [Fact]
    public async Task ResolveRecordMatchingSupportTaskAsync_WithNotConnectingOutcome_WithRequiredPolicy_DoesNotSendEmail()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

        var applicationUser = await TestData.CreateApplicationUserAsync(
            recordMatchingPolicy: RecordMatchingPolicy.Required,
            appContent: new AppContent { OneLoginNotConnectedEmailTemplateId = "custom-template-id" });

        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject, t => t.WithClientApplicationUserId(applicationUser.UserId));

        var options = new NotConnectingOutcomeOptions
        {
            SupportTask = supportTask,
            NotConnectingReason = OneLoginUserNotConnectingReason.AnotherReason,
            NotConnectingAdditionalDetails = Faker.Lorem.Paragraph()
        };

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ResolveRecordMatchingSupportTaskAsync(options, processContext));

        // Assert
        await BackgroundJobScheduler.ExecuteDeferredJobsAsync();
        var emails = await WithDbContextAsync(dbContext => dbContext.Emails.Where(e => e.EmailAddress == oneLoginUser.EmailAddress).ToArrayAsync());
        Assert.Empty(emails);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ResolveRecordMatchingSupportTaskAsync_WithNoMatchesOutcome_ClosesSupportTaskSetsOutcomeAsExpected(bool emailExpected)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

        // An email is only sent when the application user has a Required record matching policy and a
        // "cannot find record" email template configured in its app content.
        var applicationUser = emailExpected
            ? await TestData.CreateApplicationUserAsync(
                recordMatchingPolicy: RecordMatchingPolicy.Required,
                appContent: new AppContent { OneLoginCannotFindRecordEmailTemplateId = EmailTemplateIds.OneLoginCannotFindRecord })
            : await TestData.CreateApplicationUserAsync(recordMatchingPolicy: RecordMatchingPolicy.Deferred);

        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject, t => t.WithClientApplicationUserId(applicationUser.UserId));

        var data = supportTask.GetData<OneLoginUserRecordMatchingData>();

        var options = new NoMatchesOutcomeOptions
        {
            SupportTask = supportTask
        };

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        var result = await WithServiceAsync(s => s.ResolveRecordMatchingSupportTaskAsync(options, processContext));

        // Assert
        Assert.Equal(emailExpected, result.EmailSent);

        var updatedSupportTask =
            await WithDbContextAsync(dbContext => dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference));
        var updatedData = updatedSupportTask.GetData<OneLoginUserRecordMatchingData>();
        Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
        Assert.Equal(nameof(OneLoginUserRecordMatchingOutcome.NoMatches), updatedSupportTask.OutcomeLabel);
        Assert.Null(updatedData.PersonId);
        Assert.Equal(OneLoginUserRecordMatchingOutcome.NoMatches, updatedData.Outcome);

        await BackgroundJobScheduler.ExecuteDeferredJobsAsync();
        var emails = await WithDbContextAsync(dbContext => dbContext.Emails.Where(e => e.EmailAddress == oneLoginUser.EmailAddress).ToArrayAsync());

        if (emailExpected)
        {
            Assert.Collection(emails, e => Assert.Equal(EmailTemplateIds.OneLoginCannotFindRecord, e.TemplateId));
            Events.AssertEventsPublished(
                e => Assert.IsType<EmailSentEvent>(e),
                e => Assert.IsType<SupportTaskUpdatedEvent>(e));
        }
        else
        {
            Assert.Empty(emails);
            Events.AssertEventsPublished(
                e => Assert.IsType<SupportTaskUpdatedEvent>(e));
        }
    }

    [Fact]
    public async Task ResolveRecordMatchingSupportTaskAsync_WithNoMatchesOutcome_WithCustomEmailReplyToId_UsesCustomReplyTo()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

        var customReplyToId = "custom-reply-to-id";
        var applicationUser = await TestData.CreateApplicationUserAsync(
            recordMatchingPolicy: RecordMatchingPolicy.Required,
            appContent: new AppContent
            {
                OneLoginCannotFindRecordEmailTemplateId = EmailTemplateIds.OneLoginCannotFindRecord,
                SupportEmailAddressNotifyId = customReplyToId
            });

        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject, t => t.WithClientApplicationUserId(applicationUser.UserId));

        var options = new NoMatchesOutcomeOptions
        {
            SupportTask = supportTask
        };

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ResolveRecordMatchingSupportTaskAsync(options, processContext));

        // Assert
        await BackgroundJobScheduler.ExecuteDeferredJobsAsync();
        var emails = await WithDbContextAsync(dbContext => dbContext.Emails.Where(e => e.EmailAddress == oneLoginUser.EmailAddress).ToArrayAsync());
        Assert.Collection(emails, e => Assert.Equal(customReplyToId, e.EmailReplyToId));
    }

    [Fact]
    public async Task ResolveRecordMatchingSupportTaskAsync_WithConnectedOutcome_ClosesSupportTaskSetsUserToMatchedAndEmailsUser()
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress());

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithVerifiedNames([matchedPerson.FirstName, matchedPerson.LastName])
                .WithVerifiedDateOfBirth(matchedPerson.DateOfBirth)
                .WithStatedTrn(matchedPerson.Trn!));

        var options = new ConnectedOutcomeOptions
        {
            SupportTask = supportTask,
            MatchedPersonId = matchedPerson.PersonId,
            MatchedAttributes =
            [
                KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson.FirstName),
                KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson.LastName),
                KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, matchedPerson.DateOfBirth.ToString("yyyy-MM-dd")),
                KeyValuePair.Create(PersonMatchedAttribute.Trn, matchedPerson.Trn)
            ]
        };

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ResolveRecordMatchingSupportTaskAsync(options, processContext));

        // Assert
        var updatedSupportTask =
            await WithDbContextAsync(dbContext => dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference));
        var updatedData = updatedSupportTask.GetData<OneLoginUserRecordMatchingData>();
        Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
        Assert.Equal(nameof(OneLoginUserRecordMatchingOutcome.Connected), updatedSupportTask.OutcomeLabel);
        Assert.NotNull(updatedData.PersonId);
        Assert.Equal(OneLoginUserRecordMatchingOutcome.Connected, updatedData.Outcome);

        var updatedOneLoginUser =
            await WithDbContextAsync(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == oneLoginUser.Subject));
        Assert.Equal(matchedPerson.PersonId, updatedOneLoginUser.PersonId);
        Assert.Equal(TimeProvider.UtcNow, updatedOneLoginUser.MatchedOn);
        Assert.Equal(OneLoginUserMatchRoute.SupportUi, updatedOneLoginUser.MatchRoute);
        Assert.NotNull(updatedOneLoginUser.MatchedAttributes);

        await BackgroundJobScheduler.ExecuteDeferredJobsAsync();
        var emails = await WithDbContextAsync(dbContext => dbContext.Emails.Where(e => e.EmailAddress == oneLoginUser.EmailAddress).ToArrayAsync());
        Assert.Collection(emails, e => Assert.Equal(EmailTemplateIds.OneLoginRecordMatched, e.TemplateId));

        Events.AssertEventsPublished(
            e => Assert.IsType<EmailSentEvent>(e),
            e => Assert.IsType<OneLoginUserUpdatedEvent>(e),
            e => Assert.IsType<SupportTaskUpdatedEvent>(e));
    }

    [Fact]
    public async Task ResolveRecordMatchingSupportTaskAsync_WithConnectedOutcome_WithCustomEmailTemplateId_UsesCustomTemplate()
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync();

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

        var customTemplateId = "custom-template-id";
        var applicationUser = await TestData.CreateApplicationUserAsync(
            appContent: new AppContent { OneLoginRecordMatchedEmailTemplateId = customTemplateId });

        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithVerifiedNames([matchedPerson.FirstName, matchedPerson.LastName])
                .WithVerifiedDateOfBirth(matchedPerson.DateOfBirth)
                .WithStatedTrn(matchedPerson.Trn!)
                .WithClientApplicationUserId(applicationUser.UserId));

        var options = new ConnectedOutcomeOptions
        {
            SupportTask = supportTask,
            MatchedPersonId = matchedPerson.PersonId,
            MatchedAttributes =
            [
                KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson.FirstName),
                KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson.LastName),
                KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, matchedPerson.DateOfBirth.ToString("yyyy-MM-dd")),
                KeyValuePair.Create(PersonMatchedAttribute.Trn, matchedPerson.Trn)
            ]
        };

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ResolveRecordMatchingSupportTaskAsync(options, processContext));

        // Assert
        await BackgroundJobScheduler.ExecuteDeferredJobsAsync();
        var emails = await WithDbContextAsync(dbContext => dbContext.Emails.Where(e => e.EmailAddress == oneLoginUser.EmailAddress).ToArrayAsync());
        Assert.Collection(emails, e => Assert.Equal(customTemplateId, e.TemplateId));
    }

    [Fact]
    public async Task ResolveRecordMatchingSupportTaskAsync_WithConnectedOutcome_WithCustomEmailReplyToId_UsesCustomReplyTo()
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync();

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

        var customReplyToId = "custom-reply-to-id";
        var applicationUser = await TestData.CreateApplicationUserAsync(
            appContent: new AppContent { SupportEmailAddressNotifyId = customReplyToId });

        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithVerifiedNames([matchedPerson.FirstName, matchedPerson.LastName])
                .WithVerifiedDateOfBirth(matchedPerson.DateOfBirth)
                .WithStatedTrn(matchedPerson.Trn!)
                .WithClientApplicationUserId(applicationUser.UserId));

        var options = new ConnectedOutcomeOptions
        {
            SupportTask = supportTask,
            MatchedPersonId = matchedPerson.PersonId,
            MatchedAttributes =
            [
                KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson.FirstName),
                KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson.LastName),
                KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, matchedPerson.DateOfBirth.ToString("yyyy-MM-dd")),
                KeyValuePair.Create(PersonMatchedAttribute.Trn, matchedPerson.Trn)
            ]
        };

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ResolveRecordMatchingSupportTaskAsync(options, processContext));

        // Assert
        await BackgroundJobScheduler.ExecuteDeferredJobsAsync();
        var emails = await WithDbContextAsync(dbContext => dbContext.Emails.Where(e => e.EmailAddress == oneLoginUser.EmailAddress).ToArrayAsync());
        Assert.Collection(emails, e => Assert.Equal(customReplyToId, e.EmailReplyToId));
    }

    [Fact]
    public async Task ResolveRecordMatchingSupportTaskAsync_WithConnectedOutcomeAndTrnRequest_ResolvesTrnRequestAndDoesNotEmailUser()
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress());

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

        var trnRequestId = Guid.NewGuid().ToString();

        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithVerifiedNames([matchedPerson.FirstName, matchedPerson.LastName])
                .WithVerifiedDateOfBirth(matchedPerson.DateOfBirth)
                .WithStatedTrn(matchedPerson.Trn!)
                .WithTrnRequestId(trnRequestId));

        var options = new ConnectedOutcomeOptions
        {
            SupportTask = supportTask,
            MatchedPersonId = matchedPerson.PersonId,
            MatchedAttributes =
            [
                KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson.FirstName),
                KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson.LastName),
                KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, matchedPerson.DateOfBirth.ToString("yyyy-MM-dd"))
            ]
        };

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ResolveRecordMatchingSupportTaskAsync(options, processContext));

        // Assert
        var updatedSupportTask =
            await WithDbContextAsync(dbContext => dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference));
        var updatedData = updatedSupportTask.GetData<OneLoginUserRecordMatchingData>();
        Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
        Assert.Equal(nameof(OneLoginUserRecordMatchingOutcome.Connected), updatedSupportTask.OutcomeLabel);
        Assert.NotNull(updatedData.PersonId);
        Assert.Equal(OneLoginUserRecordMatchingOutcome.Connected, updatedData.Outcome);

        var updatedOneLoginUser =
            await WithDbContextAsync(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == oneLoginUser.Subject));
        Assert.Equal(matchedPerson.PersonId, updatedOneLoginUser.PersonId);
        Assert.Equal(TimeProvider.UtcNow, updatedOneLoginUser.MatchedOn);
        Assert.Equal(OneLoginUserMatchRoute.SupportUi, updatedOneLoginUser.MatchRoute);
        Assert.NotNull(updatedOneLoginUser.MatchedAttributes);

        var updatedTrnRequest =
            await WithDbContextAsync(dbContext => dbContext.TrnRequestMetadata.SingleAsync(r => r.RequestId == trnRequestId));
        Assert.Equal(matchedPerson.PersonId, updatedTrnRequest.ResolvedPersonId);
        Assert.Equal(TrnRequestStatus.Completed, updatedTrnRequest.Status);

        await BackgroundJobScheduler.ExecuteDeferredJobsAsync();
        var emails = await WithDbContextAsync(dbContext => dbContext.Emails.Where(e => e.EmailAddress == oneLoginUser.EmailAddress).ToArrayAsync());
        Assert.Empty(emails);

        Events.AssertEventsPublished(
            e => Assert.IsType<OneLoginUserUpdatedEvent>(e),
            e => Assert.IsType<SupportTaskUpdatedEvent>(e),
            e => Assert.IsType<TrnRequestUpdatedEvent>(e));
    }

    [Fact]
    public async Task ResolveRecordMatchingSupportTaskAsync_WithConnectedOutcomeAndNoTrnRequest_SendsEmailToUser()
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress());

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithVerifiedNames([matchedPerson.FirstName, matchedPerson.LastName])
                .WithVerifiedDateOfBirth(matchedPerson.DateOfBirth)
                .WithStatedTrn(matchedPerson.Trn!));

        var options = new ConnectedOutcomeOptions
        {
            SupportTask = supportTask,
            MatchedPersonId = matchedPerson.PersonId,
            MatchedAttributes =
            [
                KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson.FirstName),
                KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson.LastName)
            ]
        };

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ResolveRecordMatchingSupportTaskAsync(options, processContext));

        // Assert
        var updatedSupportTask =
            await WithDbContextAsync(dbContext => dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference));
        var updatedData = updatedSupportTask.GetData<OneLoginUserRecordMatchingData>();
        Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
        Assert.Equal(nameof(OneLoginUserRecordMatchingOutcome.Connected), updatedSupportTask.OutcomeLabel);
        Assert.NotNull(updatedData.PersonId);
        Assert.Equal(OneLoginUserRecordMatchingOutcome.Connected, updatedData.Outcome);

        var updatedOneLoginUser =
            await WithDbContextAsync(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == oneLoginUser.Subject));
        Assert.Equal(matchedPerson.PersonId, updatedOneLoginUser.PersonId);

        await BackgroundJobScheduler.ExecuteDeferredJobsAsync();
        var emails = await WithDbContextAsync(dbContext => dbContext.Emails.Where(e => e.EmailAddress == oneLoginUser.EmailAddress).ToArrayAsync());
        Assert.Collection(emails, e => Assert.Equal(EmailTemplateIds.OneLoginRecordMatched, e.TemplateId));

        Events.AssertEventsPublished(
            e => Assert.IsType<EmailSentEvent>(e),
            e => Assert.IsType<OneLoginUserUpdatedEvent>(e),
            e => Assert.IsType<SupportTaskUpdatedEvent>(e));
    }

    [Fact]
    public async Task ResolveRecordMatchingSupportTaskAsync_WithNotConnectingOutcomeAndPendingTrnRequest_ResolvesTrnRequestWithMatchedPerson()
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber());

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
        var trnRequestId = Guid.NewGuid().ToString();

        // The support task's TRN request matches an existing person on date of birth + NINO (a definite match)
        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithVerifiedNames([matchedPerson.FirstName, matchedPerson.LastName])
                .WithVerifiedDateOfBirth(matchedPerson.DateOfBirth)
                .WithStatedNationalInsuranceNumber(matchedPerson.NationalInsuranceNumber)
                .WithTrnRequestId(trnRequestId));

        // The TRN request has been activated and deferred to this support task, so it's Pending
        await SetTrnRequestToPendingAsync(supportTask);

        var options = new NotConnectingOutcomeOptions
        {
            SupportTask = supportTask,
            NotConnectingReason = OneLoginUserNotConnectingReason.AnotherReason,
            NotConnectingAdditionalDetails = Faker.Lorem.Paragraph()
        };

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ResolveRecordMatchingSupportTaskAsync(options, processContext));

        // Assert
        var updatedSupportTask =
            await WithDbContextAsync(dbContext => dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference));
        Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
        Assert.Equal(nameof(OneLoginUserRecordMatchingOutcome.NotConnecting), updatedSupportTask.OutcomeLabel);

        // Because the linked TRN request was Pending, it's resolved to the matched person
        var updatedTrnRequest = await ReloadTrnRequestAsync(supportTask);
        Assert.Equal(matchedPerson.PersonId, updatedTrnRequest.ResolvedPersonId);
        Assert.Equal(TrnRequestStatus.Completed, updatedTrnRequest.Status);
    }

    [Fact]
    public async Task ResolveRecordMatchingSupportTaskAsync_WithNoMatchesOutcomeAndPendingTrnRequest_ResolvesTrnRequestWithNewRecord()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
        var trnRequestId = Guid.NewGuid().ToString();

        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject, t => t.WithTrnRequestId(trnRequestId));

        // The TRN request has been activated and deferred to this support task, so it's Pending
        await SetTrnRequestToPendingAsync(supportTask);

        var options = new NoMatchesOutcomeOptions { SupportTask = supportTask };

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ResolveRecordMatchingSupportTaskAsync(options, processContext));

        // Assert
        var updatedSupportTask =
            await WithDbContextAsync(dbContext => dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference));
        Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
        Assert.Equal(nameof(OneLoginUserRecordMatchingOutcome.NoMatches), updatedSupportTask.OutcomeLabel);

        // Because the linked TRN request was Pending and had no match, it's resolved with a newly-created record
        var updatedTrnRequest = await ReloadTrnRequestAsync(supportTask);
        Assert.NotNull(updatedTrnRequest.ResolvedPersonId);
        Assert.Equal(TrnRequestStatus.Completed, updatedTrnRequest.Status);

        await WithDbContextAsync(async dbContext =>
        {
            var person = await dbContext.Persons.SingleOrDefaultAsync(p => p.PersonId == updatedTrnRequest.ResolvedPersonId);
            Assert.NotNull(person);
        });
    }

    [Fact]
    public async Task ResolveRecordMatchingSupportTaskAsync_WithNoMatchesOutcomeAndNonPendingTrnRequest_DoesNotResolveTrnRequest()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
        var trnRequestId = Guid.NewGuid().ToString();

        // The linked TRN request is left Dormant (it has not been activated), so resolution should not be triggered
        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject, t => t.WithTrnRequestId(trnRequestId));

        var options = new NoMatchesOutcomeOptions { SupportTask = supportTask };

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ResolveRecordMatchingSupportTaskAsync(options, processContext));

        // Assert
        var updatedSupportTask =
            await WithDbContextAsync(dbContext => dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference));
        Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
        Assert.Equal(nameof(OneLoginUserRecordMatchingOutcome.NoMatches), updatedSupportTask.OutcomeLabel);

        // The TRN request wasn't Pending, so it's untouched
        var updatedTrnRequest =
            await WithDbContextAsync(dbContext => dbContext.TrnRequestMetadata.SingleAsync(r => r.RequestId == trnRequestId));
        Assert.Null(updatedTrnRequest.ResolvedPersonId);
        Assert.Equal(TrnRequestStatus.Dormant, updatedTrnRequest.Status);
    }

    // The service resolves the TRN request via its own TrsDbContext, so re-read it rather than asserting on the
    // supportTask's stale TrnRequestMetadata
    private Task<TrnRequestMetadata> ReloadTrnRequestAsync(SupportTask supportTask) =>
        WithDbContextAsync(dbContext =>
            dbContext.TrnRequestMetadata.SingleAsync(m => m.RequestId == supportTask.TrnRequestId));

    private async Task SetTrnRequestToPendingAsync(SupportTask supportTask)
    {
        await WithDbContextAsync(async dbContext =>
        {
            var metadata = await dbContext.TrnRequestMetadata.SingleAsync(m => m.RequestId == supportTask.TrnRequestId);
            metadata.Status = TrnRequestStatus.Pending;
            await dbContext.SaveChangesAsync();
        });

        supportTask.TrnRequestMetadata!.Status = TrnRequestStatus.Pending;
    }
}

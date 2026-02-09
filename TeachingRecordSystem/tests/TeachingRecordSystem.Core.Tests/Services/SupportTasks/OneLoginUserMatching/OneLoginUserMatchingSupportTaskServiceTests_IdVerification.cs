using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

namespace TeachingRecordSystem.Core.Tests.Services.SupportTasks.OneLoginUserMatching;

public partial class OneLoginUserMatchingSupportTaskServiceTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
    [Fact]
    public async Task CreateVerificationSupportTaskAsync_CreatesSupportTaskWithExpectedData()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var statedFirstName = Faker.Name.First();
        var statedLastName = Faker.Name.Last();
        var statedDateOfBirth = DateOnly.FromDateTime(Faker.Identification.DateOfBirth());
        var statedNationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
        var statedTrn = await TestData.GenerateTrnAsync();
        var clientApplicationUserId = Guid.NewGuid();
        var trnTokenTrn = await TestData.GenerateTrnAsync();
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "evidence.jpg";

        var options = new CreateOneLoginUserIdVerificationSupportTaskOptions
        {
            OneLoginUserSubject = oneLoginUser.Subject,
            StatedNationalInsuranceNumber = statedNationalInsuranceNumber,
            StatedTrn = statedTrn,
            ClientApplicationUserId = clientApplicationUserId,
            TrnTokenTrn = trnTokenTrn,
            StatedFirstName = statedFirstName,
            StatedLastName = statedLastName,
            StatedDateOfBirth = statedDateOfBirth,
            EvidenceFileId = evidenceFileId,
            EvidenceFileName = evidenceFileName
        };

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var supportTask = await WithServiceAsync(s => s.CreateVerificationSupportTaskAsync(options, processContext));

        // Assert
        Assert.NotNull(supportTask);
        Assert.Equal(SupportTaskType.OneLoginUserIdVerification, supportTask.SupportTaskType);
        Assert.Equal(SupportTaskStatus.Open, supportTask.Status);
        Assert.Equal(oneLoginUser.Subject, supportTask.OneLoginUserSubject);
        Assert.NotNull(supportTask.SupportTaskReference);

        var data = supportTask.GetData<OneLoginUserIdVerificationData>();
        Assert.Equal(oneLoginUser.Subject, data.OneLoginUserSubject);
        Assert.Equal(statedFirstName, data.StatedFirstName);
        Assert.Equal(statedLastName, data.StatedLastName);
        Assert.Equal(statedDateOfBirth, data.StatedDateOfBirth);
        Assert.Equal(statedNationalInsuranceNumber, data.StatedNationalInsuranceNumber);
        Assert.Equal(statedTrn, data.StatedTrn);
        Assert.Equal(clientApplicationUserId, data.ClientApplicationUserId);
        Assert.Equal(trnTokenTrn, data.TrnTokenTrn);
        Assert.Equal(evidenceFileId, data.EvidenceFileId);
        Assert.Equal(evidenceFileName, data.EvidenceFileName);

        Events.AssertEventsPublished(e => Assert.IsType<SupportTaskCreatedEvent>(e));
    }

    [Fact]
    public async Task ResolveVerificationSupportTaskAsync_WithNotVerifiedOutcome_ClosesSupportTaskAndKeepsUserNotVerifiedAndNotMatchedAndEmailsUser()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var rejectReason = OneLoginIdVerificationRejectReason.AnotherReason;
        var rejectionAdditionalDetails = Faker.Lorem.Paragraph();

        var options = new NotVerifiedOutcomeOptions
        {
            SupportTask = supportTask,
            RejectReason = rejectReason,
            RejectionAdditionalDetails = rejectionAdditionalDetails
        };

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ResolveVerificationSupportTaskAsync(options, processContext));

        // Assert
        var updatedSupportTask =
            await WithDbContextAsync(dbContext => dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference));
        var updatedData = updatedSupportTask.GetData<OneLoginUserIdVerificationData>();
        Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
        Assert.False(updatedData.Verified);
        Assert.Null(updatedData.PersonId);
        Assert.Equal(OneLoginUserIdVerificationOutcome.NotVerified, updatedData.Outcome);
        Assert.Equal(rejectReason, updatedData.RejectReason);
        Assert.Equal(rejectionAdditionalDetails, updatedData.RejectionAdditionalDetails);

        var updatedOneLoginUser =
            await WithDbContextAsync(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == oneLoginUser.Subject));
        Assert.Null(updatedOneLoginUser.VerifiedOn);
        Assert.Null(updatedOneLoginUser.VerificationRoute);
        Assert.Null(updatedOneLoginUser.VerifiedNames);
        Assert.Null(updatedOneLoginUser.VerifiedDatesOfBirth);
        Assert.Null(updatedOneLoginUser.PersonId);
        Assert.Null(updatedOneLoginUser.MatchedOn);
        Assert.Null(updatedOneLoginUser.MatchRoute);
        Assert.Null(updatedOneLoginUser.MatchedAttributes);

        Events.AssertEventsPublished(e => Assert.IsType<SupportTaskUpdatedEvent>(e));

        await BackgroundJobScheduler.ExecuteDeferredJobsAsync();
        var emails = await WithDbContextAsync(dbContext => dbContext.Emails.Where(e => e.EmailAddress == oneLoginUser.EmailAddress).ToArrayAsync());
        Assert.Collection(emails, e => Assert.Equal(EmailTemplateIds.OneLoginNotVerified, e.TemplateId));

        Events.AssertEventsPublished(
            e => Assert.IsType<EmailSentEvent>(e),
            e => Assert.IsType<SupportTaskUpdatedEvent>(e));
    }

    [Theory]
    [InlineData(OneLoginIdVerificationRejectReason.ProofDoesNotMatchRequest, "The proof of identity does not match the request details")]
    [InlineData(OneLoginIdVerificationRejectReason.ProofIsUnclear, "The proof of identity is unclear")]
    [InlineData(OneLoginIdVerificationRejectReason.ProofIsWrongType, "The proof of identity is the wrong type")]
    public async Task ResolveVerificationSupportTaskAsync_WithNotVerifiedOutcome_EmailReason(
        OneLoginIdVerificationRejectReason reason,
        string expectedEmailReasonText)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var rejectionAdditionalDetails = Faker.Lorem.Paragraph();

        var options = new NotVerifiedOutcomeOptions
        {
            SupportTask = supportTask,
            RejectReason = reason,
            RejectionAdditionalDetails = rejectionAdditionalDetails
        };

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ResolveVerificationSupportTaskAsync(options, processContext));

        // Assert
        await BackgroundJobScheduler.ExecuteDeferredJobsAsync();
        var emails = await WithDbContextAsync(dbContext => dbContext.Emails.Where(e => e.EmailAddress == oneLoginUser.EmailAddress).ToArrayAsync());
        Assert.Collection(emails,
            e => Assert.Equal(e.Personalization["reason"], expectedEmailReasonText));
    }

    [Fact]
    public async Task ResolveVerificationSupportTaskAsync_WithNotVerifiedOutcome_AnotherReasonUsesReasonDetailsInEmail()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var rejectionAdditionalDetails = Faker.Lorem.Paragraph();

        var options = new NotVerifiedOutcomeOptions
        {
            SupportTask = supportTask,
            RejectReason = OneLoginIdVerificationRejectReason.AnotherReason,
            RejectionAdditionalDetails = rejectionAdditionalDetails
        };

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ResolveVerificationSupportTaskAsync(options, processContext));

        // Assert
        await BackgroundJobScheduler.ExecuteDeferredJobsAsync();
        var emails = await WithDbContextAsync(dbContext => dbContext.Emails.Where(e => e.EmailAddress == oneLoginUser.EmailAddress).ToArrayAsync());
        Assert.Collection(emails,
            e => Assert.Equal(e.Personalization["reason"], rejectionAdditionalDetails));
    }

    [Fact]
    public async Task ResolveVerificationSupportTaskAsync_WithVerifiedOnlyWithPotentialMatchesOutcome_ClosesSupportTaskSetsUserToVerifiedButNotMatched()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var data = supportTask.GetData<OneLoginUserIdVerificationData>();

        var notConnectingReason = OneLoginUserNotConnectingReason.AnotherReason;
        var notConnectingAdditionalDetails = Faker.Lorem.Paragraph();

        var options = new VerifiedOnlyWithMatchesOutcomeOptions
        {
            SupportTask = supportTask,
            NotConnectingReason = notConnectingReason,
            NotConnectingAdditionalDetails = notConnectingAdditionalDetails
        };

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ResolveVerificationSupportTaskAsync(options, processContext));

        // Assert
        var updatedSupportTask =
            await WithDbContextAsync(dbContext => dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference));
        var updatedData = updatedSupportTask.GetData<OneLoginUserIdVerificationData>();
        Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
        Assert.True(updatedData.Verified);
        Assert.Null(updatedData.PersonId);
        Assert.Equal(OneLoginUserIdVerificationOutcome.VerifiedOnlyWithMatches, updatedData.Outcome);
        Assert.Equal(notConnectingReason, updatedData.NotConnectingReason);
        Assert.Equal(notConnectingAdditionalDetails, updatedData.NotConnectingAdditionalDetails);

        var updatedOneLoginUser =
            await WithDbContextAsync(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == oneLoginUser.Subject));
        Assert.NotNull(updatedOneLoginUser.VerifiedOn);
        Assert.Equal(OneLoginUserVerificationRoute.Support, updatedOneLoginUser.VerificationRoute);
        Assert.NotNull(updatedOneLoginUser.VerifiedNames);
        Assert.Collection(updatedOneLoginUser.VerifiedNames, names => Assert.Equivalent(new[] { data.StatedFirstName, data.StatedLastName }, names));
        Assert.NotNull(updatedOneLoginUser.VerifiedDatesOfBirth);
        Assert.Collection(updatedOneLoginUser.VerifiedDatesOfBirth, dob => Assert.Equal(data.StatedDateOfBirth, dob));
        Assert.Null(updatedOneLoginUser.PersonId);
        Assert.Null(updatedOneLoginUser.MatchedOn);
        Assert.Null(updatedOneLoginUser.MatchRoute);
        Assert.Null(updatedOneLoginUser.MatchedAttributes);

        Events.AssertEventsPublished(
            e => Assert.IsType<OneLoginUserUpdatedEvent>(e),
            e => Assert.IsType<SupportTaskUpdatedEvent>(e));
    }

    [Fact]
    public async Task ResolveVerificationSupportTaskAsync_WithVerifiedOnlyWithoutPotentialMatchesOutcome_ClosesSupportTaskSetsUserToVerifiedButNotMatchedAndEmailsUser()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var data = supportTask.GetData<OneLoginUserIdVerificationData>();

        var options = new VerifiedOnlyWithoutMatchesOutcomeOptions
        {
            SupportTask = supportTask
        };

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ResolveVerificationSupportTaskAsync(options, processContext));

        // Assert
        var updatedSupportTask =
            await WithDbContextAsync(dbContext => dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference));
        var updatedData = updatedSupportTask.GetData<OneLoginUserIdVerificationData>();
        Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
        Assert.True(updatedData.Verified);
        Assert.Null(updatedData.PersonId);
        Assert.Equal(OneLoginUserIdVerificationOutcome.VerifiedOnlyWithoutMatches, updatedData.Outcome);

        var updatedOneLoginUser =
            await WithDbContextAsync(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == oneLoginUser.Subject));
        Assert.NotNull(updatedOneLoginUser.VerifiedOn);
        Assert.Equal(OneLoginUserVerificationRoute.Support, updatedOneLoginUser.VerificationRoute);
        Assert.NotNull(updatedOneLoginUser.VerifiedNames);
        Assert.Collection(updatedOneLoginUser.VerifiedNames, names => Assert.Equivalent(new[] { data.StatedFirstName, data.StatedLastName }, names));
        Assert.NotNull(updatedOneLoginUser.VerifiedDatesOfBirth);
        Assert.Collection(updatedOneLoginUser.VerifiedDatesOfBirth, dob => Assert.Equal(data.StatedDateOfBirth, dob));
        Assert.Null(updatedOneLoginUser.PersonId);
        Assert.Null(updatedOneLoginUser.MatchedOn);
        Assert.Null(updatedOneLoginUser.MatchRoute);
        Assert.Null(updatedOneLoginUser.MatchedAttributes);

        await BackgroundJobScheduler.ExecuteDeferredJobsAsync();
        var emails = await WithDbContextAsync(dbContext => dbContext.Emails.Where(e => e.EmailAddress == oneLoginUser.EmailAddress).ToArrayAsync());
        Assert.Collection(emails, e => Assert.Equal(EmailTemplateIds.OneLoginCannotFindRecord, e.TemplateId));

        Events.AssertEventsPublished(
            e => Assert.IsType<EmailSentEvent>(e),
            e => Assert.IsType<OneLoginUserUpdatedEvent>(e),
            e => Assert.IsType<SupportTaskUpdatedEvent>(e));
    }

    [Fact]
    public async Task ResolveVerificationSupportTaskAsync_WithVerifiedAndConnectedOutcome_ClosesSupportTaskSetsUserToVerifiedAndMatchedAndEmailsUser()
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress());

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject,
            t => t
                .WithStatedFirstName(matchedPerson.FirstName)
                .WithStatedLastName(matchedPerson.LastName)
                .WithStatedDateOfBirth(matchedPerson.DateOfBirth)
                .WithStatedTrn(matchedPerson.Trn!));

        var data = supportTask.GetData<OneLoginUserIdVerificationData>();

        var options = new VerifiedAndConnectedOutcomeOptions
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

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ResolveVerificationSupportTaskAsync(options, processContext));

        // Assert
        var updatedSupportTask =
            await WithDbContextAsync(dbContext => dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference));
        var updatedData = updatedSupportTask.GetData<OneLoginUserIdVerificationData>();
        Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
        Assert.True(updatedData.Verified);
        Assert.NotNull(updatedData.PersonId);
        Assert.Equal(OneLoginUserIdVerificationOutcome.VerifiedAndConnected, updatedData.Outcome);

        var updatedOneLoginUser =
            await WithDbContextAsync(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == oneLoginUser.Subject));
        Assert.NotNull(updatedOneLoginUser.VerifiedOn);
        Assert.Equal(OneLoginUserVerificationRoute.Support, updatedOneLoginUser.VerificationRoute);
        Assert.NotNull(updatedOneLoginUser.VerifiedNames);
        Assert.Collection(updatedOneLoginUser.VerifiedNames, names => Assert.Equivalent(new[] { data.StatedFirstName, data.StatedLastName }, names));
        Assert.NotNull(updatedOneLoginUser.VerifiedDatesOfBirth);
        Assert.Collection(updatedOneLoginUser.VerifiedDatesOfBirth, dob => Assert.Equal(data.StatedDateOfBirth, dob));
        Assert.Equal(matchedPerson.PersonId, updatedOneLoginUser.PersonId);
        Assert.Equal(Clock.UtcNow, updatedOneLoginUser.MatchedOn);
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

    private Task WithServiceAsync(Func<OneLoginUserMatchingSupportTaskService, Task> action, params object[] arguments) =>
        WithServiceAsync<OneLoginUserMatchingSupportTaskService>(action, arguments);

    private Task<TResult> WithServiceAsync<TResult>(Func<OneLoginUserMatchingSupportTaskService, Task<TResult>> action, params object[] arguments) =>
        WithServiceAsync<OneLoginUserMatchingSupportTaskService, TResult>(action, arguments);
}

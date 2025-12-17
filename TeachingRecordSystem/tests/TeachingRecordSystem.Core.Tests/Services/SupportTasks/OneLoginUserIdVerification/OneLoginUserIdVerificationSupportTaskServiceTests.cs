using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserIdVerification;

namespace TeachingRecordSystem.Core.Tests.Services.SupportTasks.OneLoginUserIdVerification;

public class OneLoginUserIdVerificationSupportTaskServiceTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
    [Fact]
    public async Task ResolveSupportTaskAsync_WithNotVerifiedOutcome_ClosesSupportTaskAndKeepsUserNotVerifiedAndNotMatched()
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
        await WithServiceAsync(s => s.ResolveSupportTaskAsync(options, processContext));

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
    }

    [Fact]
    public async Task ResolveSupportTaskAsync_WithVerifiedOnlyWithPotentialMatchesOutcome_ClosesSupportTaskSetsUserToVerifiedButNotMatched()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var data = supportTask.GetData<OneLoginUserIdVerificationData>();

        var notConnectingReason = OneLoginIdVerificationNotConnectingReason.AnotherReason;
        var notConnectingAdditionalDetails = Faker.Lorem.Paragraph();

        var options = new VerifiedOnlyWithMatchesOutcomeOptions
        {
            SupportTask = supportTask,
            NotConnectingReason = notConnectingReason,
            NotConnectingAdditionalDetails = notConnectingAdditionalDetails
        };

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ResolveSupportTaskAsync(options, processContext));

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

        Events.AssertEventsPublished(e => Assert.IsType<SupportTaskUpdatedEvent>(e));
    }

    [Fact]
    public async Task ResolveSupportTaskAsync_WithVerifiedOnlyWithoutPotentialMatchesOutcome_ClosesSupportTaskSetsUserToVerifiedButNotMatchedAndEmailsUser()
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
        await WithServiceAsync(s => s.ResolveSupportTaskAsync(options, processContext));

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
            e => Assert.IsType<SupportTaskUpdatedEvent>(e),
            e => Assert.IsType<EmailSentEvent>(e));
    }

    [Fact]
    public async Task ResolveSupportTaskAsync_WithVerifiedAndConnectedOutcome_ClosesSupportTaskSetsUserToVerifiedAndMatchedAndEmailsUser()
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
        await WithServiceAsync(s => s.ResolveSupportTaskAsync(options, processContext));

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
        Assert.Equal(OneLoginUserMatchRoute.Support, updatedOneLoginUser.MatchRoute);
        Assert.NotNull(updatedOneLoginUser.MatchedAttributes);

        await BackgroundJobScheduler.ExecuteDeferredJobsAsync();
        var emails = await WithDbContextAsync(dbContext => dbContext.Emails.Where(e => e.EmailAddress == oneLoginUser.EmailAddress).ToArrayAsync());
        Assert.Collection(emails, e => Assert.Equal(EmailTemplateIds.OneLoginRecordMatched, e.TemplateId));

        Events.AssertEventsPublished(
            e => Assert.IsType<SupportTaskUpdatedEvent>(e),
            e => Assert.IsType<EmailSentEvent>(e));
    }

    private Task WithServiceAsync(Func<OneLoginUserIdVerificationSupportTaskService, Task> action, params object[] arguments) =>
        WithServiceAsync<OneLoginUserIdVerificationSupportTaskService>(action, arguments);

    private Task<TResult> WithServiceAsync<TResult>(Func<OneLoginUserIdVerificationSupportTaskService, Task<TResult>> action, params object[] arguments) =>
        WithServiceAsync<OneLoginUserIdVerificationSupportTaskService, TResult>(action, arguments);
}

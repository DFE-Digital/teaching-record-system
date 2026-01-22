using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserRecordMatching;

namespace TeachingRecordSystem.Core.Tests.Services.SupportTasks.OneLoginUserMatching;

public partial class OneLoginUserMatchingSupportTaskServiceTests
{
    [Fact]
    public async Task ResolveSupportTaskAsync_WithNotConnectingOutcome_ClosesSupportTaskSetsOutcomeAsExpected()
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

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ResolveSupportTaskAsync(options, processContext));

        // Assert
        var updatedSupportTask =
            await WithDbContextAsync(dbContext => dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference));
        var updatedData = updatedSupportTask.GetData<OneLoginUserRecordMatchingData>();
        Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
        Assert.True(updatedData.Verified);
        Assert.Null(updatedData.PersonId);
        Assert.Equal(OneLoginUserRecordMatchingOutcome.NotConnecting, updatedData.Outcome);
        Assert.Equal(notConnectingReason, updatedData.NotConnectingReason);
        Assert.Equal(notConnectingAdditionalDetails, updatedData.NotConnectingAdditionalDetails);

        Events.AssertEventsPublished(e => Assert.IsType<SupportTaskUpdatedEvent>(e));
    }

    [Fact]
    public async Task ResolveSupportTaskAsync_WithNoMatchesOutcome_ClosesSupportTaskSetsOutcomeAsExpectedAndEmailsUser()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject);

        var data = supportTask.GetData<OneLoginUserRecordMatchingData>();

        var options = new NoMatchesOutcomeOptions
        {
            SupportTask = supportTask
        };

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ResolveSupportTaskAsync(options, processContext));

        // Assert
        var updatedSupportTask =
            await WithDbContextAsync(dbContext => dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference));
        var updatedData = updatedSupportTask.GetData<OneLoginUserRecordMatchingData>();
        Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
        Assert.True(updatedData.Verified);
        Assert.Null(updatedData.PersonId);
        Assert.Equal(OneLoginUserRecordMatchingOutcome.NoMatches, updatedData.Outcome);

        await BackgroundJobScheduler.ExecuteDeferredJobsAsync();
        var emails = await WithDbContextAsync(dbContext => dbContext.Emails.Where(e => e.EmailAddress == oneLoginUser.EmailAddress).ToArrayAsync());
        Assert.Collection(emails, e => Assert.Equal(EmailTemplateIds.OneLoginCannotFindRecord, e.TemplateId));

        Events.AssertEventsPublished(
            e => Assert.IsType<SupportTaskUpdatedEvent>(e),
            e => Assert.IsType<EmailSentEvent>(e));
    }

    [Fact]
    public async Task ResolveSupportTaskAsync_WithConnectedOutcome_ClosesSupportTaskSetsUserToMatchedAndEmailsUser()
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

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(s => s.ResolveSupportTaskAsync(options, processContext));

        // Assert
        var updatedSupportTask =
            await WithDbContextAsync(dbContext => dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference));
        var updatedData = updatedSupportTask.GetData<OneLoginUserRecordMatchingData>();
        Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
        Assert.NotNull(updatedData.PersonId);
        Assert.Equal(OneLoginUserRecordMatchingOutcome.Connected, updatedData.Outcome);

        var updatedOneLoginUser =
            await WithDbContextAsync(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == oneLoginUser.Subject));
        Assert.Equal(matchedPerson.PersonId, updatedOneLoginUser.PersonId);
        Assert.Equal(Clock.UtcNow, updatedOneLoginUser.MatchedOn);
        Assert.Equal(OneLoginUserMatchRoute.SupportUi, updatedOneLoginUser.MatchRoute);
        Assert.NotNull(updatedOneLoginUser.MatchedAttributes);

        await BackgroundJobScheduler.ExecuteDeferredJobsAsync();
        var emails = await WithDbContextAsync(dbContext => dbContext.Emails.Where(e => e.EmailAddress == oneLoginUser.EmailAddress).ToArrayAsync());
        Assert.Collection(emails, e => Assert.Equal(EmailTemplateIds.OneLoginRecordMatched, e.TemplateId));

        Events.AssertEventsPublished(
            e => Assert.IsType<SupportTaskUpdatedEvent>(e),
            e => Assert.IsType<EmailSentEvent>(e));
    }
}

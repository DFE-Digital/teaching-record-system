using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserMatching.Resolve;

public class ConfirmNotConnectingTests(HostFixture hostFixture) : ResolveOneLoginUserMatchingTestBase(hostFixture)
{
    [Theory]
    [InlineData(false)]
    [InlineData(null)]
    public async Task Get_UserIsNotVerified_RedirectsToIndex(bool? verified)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s => s.Verified = verified);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "Reason", nameof(OneLoginUserNotConnectingReason.NoMatchingRecord) } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_MatchedPersonIsNotSentinel_RedirectsToMatches(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: isRecordMatchingOnlySupportTask);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = true;
                s.MatchedPersonId = Guid.NewGuid();
                s.NotConnectingReason = OneLoginUserNotConnectingReason.NoMatchingRecord;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_ReasonNotChosen_RedirectsToNotConnectingPage(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: isRecordMatchingOnlySupportTask);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = true;
                s.MatchedPersonId = ResolveOneLoginUserMatchingState.NotMatchedPersonIdSentinel;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_ShowsExpectedData(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: isRecordMatchingOnlySupportTask);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var notConnectingReason = OneLoginUserNotConnectingReason.AnotherReason;
        var additionalDetails = Faker.Lorem.Paragraph();

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = true;
                s.MatchedPersonId = ResolveOneLoginUserMatchingState.NotMatchedPersonIdSentinel;
                s.NotConnectingReason = notConnectingReason;
                s.NotConnectingAdditionalDetails = additionalDetails;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        Assert.Equal(oneLoginUser.EmailAddress, doc.GetSummaryListValueByKey("Email address"));
        Assert.Equal(notConnectingReason.GetDisplayName(), doc.GetSummaryListValueByKey("Reason"));
        Assert.Equal(additionalDetails, doc.GetSummaryListValueByKey("Additional details"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(null)]
    public async Task Post_UserIsNotVerified_RedirectsToIndex(bool? verified)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s => s.Verified = verified);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "Reason", nameof(OneLoginUserNotConnectingReason.NoMatchingRecord) } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Post_MatchedPersonIsNotSentinel_RedirectsToMatches(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: isRecordMatchingOnlySupportTask);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = true;
                s.MatchedPersonId = Guid.NewGuid();
                s.NotConnectingReason = OneLoginUserNotConnectingReason.NoMatchingRecord;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Post_ReasonNotChosen_RedirectsToNotConnectingPage(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: isRecordMatchingOnlySupportTask);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = true;
                s.MatchedPersonId = ResolveOneLoginUserMatchingState.NotMatchedPersonIdSentinel;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_IdVerificationSupportTaskMarksUserVerifiedClosesSupportTaskAndRedirectsToListPageWithFlashMessage()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);
        var supportTaskData = supportTask.GetData<OneLoginUserIdVerificationData>();

        var notConnectingReason = OneLoginUserNotConnectingReason.AnotherReason;
        var additionalDetails = Faker.Lorem.Paragraph();

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = true;
                s.MatchedPersonId = ResolveOneLoginUserMatchingState.NotMatchedPersonIdSentinel;
                s.NotConnectingReason = notConnectingReason;
                s.NotConnectingAdditionalDetails = additionalDetails;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedSupportTask = await
                dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
            var updatedSupportTaskData = updatedSupportTask.GetData<OneLoginUserIdVerificationData>();
            Assert.Equal(OneLoginUserIdVerificationOutcome.VerifiedOnlyWithMatches, updatedSupportTaskData.Outcome);
            Assert.Equal(notConnectingReason, updatedSupportTaskData.NotConnectingReason);
            Assert.Equal(additionalDetails, updatedSupportTaskData.NotConnectingAdditionalDetails);

            var updatedOneLoginUser = await dbContext.OneLoginUsers.SingleAsync(o => o.Subject == oneLoginUser.Subject);
            Assert.Equal(Clock.UtcNow, updatedOneLoginUser.VerifiedOn);
        });

        Events.AssertProcessesCreated(p => Assert.Equal(ProcessType.OneLoginUserIdVerificationSupportTaskCompleting, p.ProcessContext.ProcessType));

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(
            nextPageDoc,
            "GOV.UK One Login not connected to a record",
            $"Request closed for {supportTaskData.StatedFirstName} {supportTaskData.StatedLastName}.");

        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal("/support-tasks/one-login-user-matching/id-verification", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_RecordMatchingSupportTaskSetsOutcomeClosesSupportTaskAndRedirectsToListPageWithFlashMessage()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
        var firstName = TestData.GenerateFirstName();
        var lastName = TestData.GenerateLastName();
        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithVerifiedNames([firstName, lastName]));
        var supportTaskData = supportTask.GetData<OneLoginUserRecordMatchingData>();

        var notConnectingReason = OneLoginUserNotConnectingReason.AnotherReason;
        var additionalDetails = Faker.Lorem.Paragraph();

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = true;
                s.MatchedPersonId = ResolveOneLoginUserMatchingState.NotMatchedPersonIdSentinel;
                s.NotConnectingReason = notConnectingReason;
                s.NotConnectingAdditionalDetails = additionalDetails;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedSupportTask = await
                dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
            var updatedSupportTaskData = updatedSupportTask.GetData<OneLoginUserRecordMatchingData>();
            Assert.Equal(OneLoginUserRecordMatchingOutcome.NotConnecting, updatedSupportTaskData.Outcome);
            Assert.Equal(notConnectingReason, updatedSupportTaskData.NotConnectingReason);
            Assert.Equal(additionalDetails, updatedSupportTaskData.NotConnectingAdditionalDetails);
        });

        Events.AssertProcessesCreated(p => Assert.Equal(ProcessType.OneLoginUserRecordMatchingSupportTaskCompleting, p.ProcessContext.ProcessType));

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(
            nextPageDoc,
            "GOV.UK One Login not connected to a record",
            $"Request closed for {firstName} {lastName}.");

        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal("/support-tasks/one-login-user-matching/record-matching", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Post_Cancel_DeletesJourneyAndRedirectToListPage(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: isRecordMatchingOnlySupportTask);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = true;
                s.MatchedPersonId = ResolveOneLoginUserMatchingState.NotMatchedPersonIdSentinel;
                s.NotConnectingReason = OneLoginUserNotConnectingReason.NoMatchingRecord;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "cancel", "true" } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        if (isRecordMatchingOnlySupportTask)
        {
            Assert.Equal(
                "/support-tasks/one-login-user-matching/record-matching",
                response.Headers.Location?.OriginalString);
        }
        else
        {
            Assert.Equal(
                "/support-tasks/one-login-user-matching/id-verification",
                response.Headers.Location?.OriginalString);
        }

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }
}

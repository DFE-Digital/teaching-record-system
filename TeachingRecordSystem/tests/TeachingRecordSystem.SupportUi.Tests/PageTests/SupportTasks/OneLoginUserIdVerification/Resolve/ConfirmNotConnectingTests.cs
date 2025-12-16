using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserIdVerification.Resolve;

public class ConfirmNotConnectingTests(HostFixture hostFixture) : ResolveOneLoginUserIdVerificationTestBase(hostFixture)
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
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/confirm-not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "Reason", nameof(OneLoginIdVerificationNotConnectingReason.NoMatchingRecord) } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_MatchedPersonIsNotSentinel_RedirectsToMatches()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = true;
                s.MatchedPersonId = Guid.NewGuid();
                s.NotConnectingReason = OneLoginIdVerificationNotConnectingReason.NoMatchingRecord;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/confirm-not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ReasonNotChosen_RedirectsToNotConnectingPage()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = true;
                s.MatchedPersonId = ResolveOneLoginUserIdVerificationState.NotMatchedPersonIdSentinel;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/confirm-not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ShowsExpectedData()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var notConnectingReason = OneLoginIdVerificationNotConnectingReason.AnotherReason;
        var additionalDetails = Faker.Lorem.Paragraph();

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = true;
                s.MatchedPersonId = ResolveOneLoginUserIdVerificationState.NotMatchedPersonIdSentinel;
                s.NotConnectingReason = notConnectingReason;
                s.NotConnectingAdditionalDetails = additionalDetails;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/confirm-not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}");

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
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/confirm-not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "Reason", nameof(OneLoginIdVerificationNotConnectingReason.NoMatchingRecord) } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_MatchedPersonIsNotSentinel_RedirectsToMatches()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = true;
                s.MatchedPersonId = Guid.NewGuid();
                s.NotConnectingReason = OneLoginIdVerificationNotConnectingReason.NoMatchingRecord;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/confirm-not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_ReasonNotChosen_RedirectsToNotConnectingPage()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = true;
                s.MatchedPersonId = ResolveOneLoginUserIdVerificationState.NotMatchedPersonIdSentinel;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/confirm-not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_MarksUserVerifiedClosesSupportTaskAndRedirectsToListPageWithFlashMessage()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);
        var supportTaskData = supportTask.GetData<OneLoginUserIdVerificationData>();

        var notConnectingReason = OneLoginIdVerificationNotConnectingReason.AnotherReason;
        var additionalDetails = Faker.Lorem.Paragraph();

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = true;
                s.MatchedPersonId = ResolveOneLoginUserIdVerificationState.NotMatchedPersonIdSentinel;
                s.NotConnectingReason = notConnectingReason;
                s.NotConnectingAdditionalDetails = additionalDetails;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/confirm-not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedSupportTask = await
                dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
            Assert.Equal(Clock.UtcNow, updatedSupportTask.UpdatedOn);
            var updatedSupportTaskData = updatedSupportTask.GetData<OneLoginUserIdVerificationData>();
            Assert.True(updatedSupportTaskData.Verified);
            Assert.Equal(OneLoginUserIdVerificationOutcome.VerifiedOnly, updatedSupportTaskData.Outcome);
            Assert.Equal(notConnectingReason, updatedSupportTaskData.NotConnectingReason);
            Assert.Equal(additionalDetails, updatedSupportTaskData.NotConnectingAdditionalDetails);

            var updatedOneLoginUser = await dbContext.OneLoginUsers.SingleAsync(o => o.Subject == oneLoginUser.Subject);
            Assert.Equal(Clock.UtcNow, updatedOneLoginUser.VerifiedOn);
            Assert.Equal(OneLoginUserVerificationRoute.Support, updatedOneLoginUser.VerificationRoute);
            Assert.NotNull(updatedOneLoginUser.VerifiedDatesOfBirth);
            Assert.Collection(updatedOneLoginUser.VerifiedDatesOfBirth, dob => Assert.Equal(supportTaskData.StatedDateOfBirth, dob));
            Assert.NotNull(updatedOneLoginUser.VerifiedNames);
            Assert.Collection(
                updatedOneLoginUser.VerifiedNames,
                names => Assert.Equivalent(new[] { supportTaskData.StatedFirstName, supportTaskData.StatedLastName }, names));
            Assert.Null(updatedOneLoginUser.PersonId);
            Assert.Null(updatedOneLoginUser.MatchedOn);
            Assert.Null(updatedOneLoginUser.MatchRoute);
            Assert.Null(updatedOneLoginUser.MatchedAttributes);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.OneLoginUserIdVerificationSupportTaskCompleting, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvents<SupportTaskUpdatedEvent>();
        });

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(
            nextPageDoc,
            "GOV.UK One Login account not connected to a record",
            $"Request closed for {supportTaskData.StatedFirstName} {supportTaskData.StatedLastName}.");

        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal("/support-tasks/one-login-user-id-verification", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectToListPage()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = true;
                s.MatchedPersonId = ResolveOneLoginUserIdVerificationState.NotMatchedPersonIdSentinel;
                s.NotConnectingReason = OneLoginIdVerificationNotConnectingReason.NoMatchingRecord;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/confirm-not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "cancel", "true" } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            "/support-tasks/one-login-user-id-verification",
            response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }
}

using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserMatching.Resolve;

public class ConfirmRejectTests(HostFixture hostFixture) : ResolveOneLoginUserMatchingTestBase(hostFixture)
{
    [Theory]
    [InlineData(true)]
    [InlineData(null)]
    public async Task Get_UserIsNotUnverified_RedirectsToIndex(bool? verified)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s => s.Verified = verified);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-reject?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ReasonNotChosen_RedirectsToRejectPage()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = false;
                s.RejectReason = null;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-reject?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/reject?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ShowsExpectedData()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var rejectReason = OneLoginIdVerificationRejectReason.AnotherReason;
        var additionalDetails = Faker.Lorem.Paragraph();

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = false;
                s.RejectReason = rejectReason;
                s.RejectionAdditionalDetails = additionalDetails;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-reject?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        Assert.Equal(oneLoginUser.EmailAddress, doc.GetSummaryListValueByKey("Email address"));
        Assert.Equal(rejectReason.GetDisplayName(), doc.GetSummaryListValueByKey("Reason"));
        Assert.Equal(additionalDetails, doc.GetSummaryListValueByKey("Additional details"));
    }

    [Fact]
    public async Task Post_LeavesUserUnverifiedClosesSupportTaskAndRedirectsToListPageWithFlashMessage()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);
        var supportTaskData = supportTask.GetData<OneLoginUserIdVerificationData>();

        var rejectReason = OneLoginIdVerificationRejectReason.AnotherReason;
        var additionalDetails = Faker.Lorem.Paragraph();

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = false;
                s.RejectReason = rejectReason;
                s.RejectionAdditionalDetails = additionalDetails;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-reject?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedSupportTask = await
                dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
            var updatedSupportTaskData = updatedSupportTask.GetData<OneLoginUserIdVerificationData>();
            Assert.Equal(OneLoginUserIdVerificationOutcome.NotVerified, updatedSupportTaskData.Outcome);
        });

        Events.AssertProcessesCreated(p => Assert.Equal(ProcessType.OneLoginUserIdVerificationSupportTaskCompleting, p.ProcessContext.ProcessType));

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(
            nextPageDoc,
            "GOV.UK One Login verification request rejected",
            $"Request closed for {supportTaskData.StatedFirstName} {supportTaskData.StatedLastName}. We’ve sent them an email confirming we could not verify their identity.");

        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal("/support-tasks/one-login-user-matching/id-verification", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectToListPage()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var rejectReason = OneLoginIdVerificationRejectReason.AnotherReason;
        var additionalDetails = Faker.Lorem.Paragraph();

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = false;
                s.RejectReason = rejectReason;
                s.RejectionAdditionalDetails = additionalDetails;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-reject?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "cancel", "true" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            "/support-tasks/one-login-user-matching/id-verification",
            response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }
}

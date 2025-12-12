using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserIdVerification.Resolve;

public class RejectTests(HostFixture hostFixture) : ResolveOneLoginUserIdVerificationTestBase(hostFixture)
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
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/reject?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequest_ReturnsOk()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s => s.Verified = false);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/reject?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseAsync(response);
    }

    [Fact]
    public async Task Get_ValidRequestWithDataInState_PopulatesFields()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var additionalDetails = Faker.Lorem.Paragraph();
        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = false;
                s.RejectReason = OneLoginIdVerificationRejectReason.AnotherReason;
                s.RejectionAdditionalDetails = additionalDetails;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/reject?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.True(doc.GetElementsByName("Reason").SingleOrDefault(e => e.GetAttribute("value") == nameof(OneLoginIdVerificationRejectReason.AnotherReason))!.HasAttribute("checked"));
        Assert.Equal(additionalDetails, doc.GetElementsByName("AdditionalDetails").Single().TrimmedText());
    }

    [Fact]
    public async Task Post_NoReasonChosen_ShowsError()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s => s.Verified = false);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/reject?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Reason", "Select a reason");
    }

    [Fact]
    public async Task Post_AnotherReasonChosenAndNoAdditionalDetails_ShowsError()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s => s.Verified = false);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/reject?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Reason", nameof(OneLoginIdVerificationRejectReason.AnotherReason) }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "AdditionalDetails", "Enter additional detail");
    }

    [Fact]
    public async Task Post_ValidRequest_UpdatesStateAndRedirects()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s => s.Verified = false);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/reject?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Reason", nameof(OneLoginIdVerificationRejectReason.ProofDoesNotMatchRequest) }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/confirm-reject?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(OneLoginIdVerificationRejectReason.ProofDoesNotMatchRequest, journeyInstance.State.RejectReason);
    }

    [Fact]
    public async Task Post_ValidRequestWithAnotherReason_UpdatesStateAndRedirects()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var additionalDetails = Faker.Lorem.Paragraph();
        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s => s.Verified = false);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/reject?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Reason", nameof(OneLoginIdVerificationRejectReason.AnotherReason) },
                { "AdditionalDetails", additionalDetails }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/confirm-reject?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(OneLoginIdVerificationRejectReason.AnotherReason, journeyInstance.State.RejectReason);
        Assert.Equal(additionalDetails, journeyInstance.State.RejectionAdditionalDetails);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectToListPage()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var additionalDetails = Faker.Lorem.Paragraph();
        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s => s.Verified = false);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/reject?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Cancel", "true" }
            }
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

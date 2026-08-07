using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserMatching.Resolve;

public class RejectTests(HostFixture hostFixture) : ResolveOneLoginUserMatchingTestBase(hostFixture)
{
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
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/reject?{journeyInstance.GetUniqueIdQueryParameter()}");

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
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/reject?{journeyInstance.GetUniqueIdQueryParameter()}");

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
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/reject?{journeyInstance.GetUniqueIdQueryParameter()}");

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
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/reject?{journeyInstance.GetUniqueIdQueryParameter()}")
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
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/reject?{journeyInstance.GetUniqueIdQueryParameter()}")
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
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-reject?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);

        var journeyState = GetJourneyInstanceState(journeyInstance);
        Assert.Equal(OneLoginIdVerificationRejectReason.ProofDoesNotMatchRequest, journeyState!.RejectReason);
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
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/reject?{journeyInstance.GetUniqueIdQueryParameter()}")
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
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-reject?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);

        var journeyState = GetJourneyInstanceState(journeyInstance);
        Assert.Equal(OneLoginIdVerificationRejectReason.AnotherReason, journeyState!.RejectReason);
        Assert.Equal(additionalDetails, journeyState!.RejectionAdditionalDetails);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Post_SaveAndComeBackLater_PersistsJourneyStateIntoTaskAndRedirectsToCorrectPage(bool supportTaskDashboardEnabled)
    {
        // Arrange
        FeatureProvider.Features.Clear();
        if (supportTaskDashboardEnabled)
        {
            FeatureProvider.Features.Add("SupportTaskDashboard");
        }

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var reason = OneLoginIdVerificationRejectReason.AnotherReason;
        var additionalDetails = Faker.Lorem.Paragraph();
        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s => s.Verified = false);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/reject?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Action", "SaveAndComeBackLater" },
                { "Reason", reason.ToString() },
                { "AdditionalDetails", additionalDetails }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        if (supportTaskDashboardEnabled)
        {
            Assert.Equal($"/support-tasks/{supportTask.SupportTaskReference}", response.Headers.Location?.OriginalString);
        }
        else
        {
            Assert.Equal($"/support-tasks/one-login-user-matching/id-verification", response.Headers.Location?.OriginalString);
        }

        await WithDbContextAsync(async dbContext =>
        {
            supportTask = (await dbContext.SupportTasks.FindAsync(supportTask.SupportTaskReference))!;
            Assert.NotNull(supportTask.ResolveJourneySavedState);

            Assert.Equal("Reject", supportTask.ResolveJourneySavedState.PageName);

            Assert.Collection(
                supportTask.ResolveJourneySavedState.ModelStateValues,
                kvp =>
                {
                    Assert.Equal("Reason", kvp.Key);
                    Assert.Equal(reason.ToString(), kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal("AdditionalDetails", kvp.Key);
                    Assert.Equal(additionalDetails, kvp.Value);
                });

            var savedState = supportTask.ResolveJourneySavedState.GetState<ResolveOneLoginUserMatchingState>();
            Assert.NotNull(savedState);
            Assert.False(savedState.Verified);
        });

        Assert.Null(GetJourneyInstanceState(journeyInstance));

        Events.AssertProcessesCreated(p => Assert.Equal(
            ProcessType.OneLoginUserIdVerificationSupportTaskSaving,
            p.ProcessContext.ProcessType));
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
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/reject?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Action", "Cancel" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            "/support-tasks/one-login-user-matching/id-verification",
            response.Headers.Location?.OriginalString);

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }
}

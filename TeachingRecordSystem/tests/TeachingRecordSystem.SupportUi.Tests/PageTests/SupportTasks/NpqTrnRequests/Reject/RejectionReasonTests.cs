using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Reject;
using TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.NpqTrnRequests.Resolve;
using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.NpqTrnRequests.Reject;

public class RejectionReasonTests(HostFixture hostFixture) : NpqTrnRequestTestBase(hostFixture)
{
    [Fact]
    public async Task Get_HasBackLinkExpected()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync("NPQ");

        var supportTask = await new CreateNpqTrnRequestSupportTaskBuilder(applicationUser.UserId)
            .WithMatches(false)
            .ExecuteAsync(TestData);

        var state = new RejectNpqTrnRequestState
        {
            RejectionReason = RejectionReasonOption.EvidenceDoesNotMatch,
        };
        var journeyInstance = await CreateJourneyInstance(
                JourneyNames.RejectNpqTrnRequest,
                state,
                new KeyValuePair<string, object>("supportTaskReference", supportTask.SupportTaskReference));

        var expectedBackLink = $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/details";

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/reject/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(expectedBackLink, doc.GetElementsByClassName("govuk-back-link").Single().GetAttribute("href"));
    }

    [Fact]
    public async Task Get_ShowsSelectedReasonFromJourneyState()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync("NPQ");

        var supportTask = await new CreateNpqTrnRequestSupportTaskBuilder(applicationUser.UserId)
            .WithMatches(false)
            .ExecuteAsync(TestData);

        var state = new RejectNpqTrnRequestState
        {
            RejectionReason = RejectionReasonOption.EvidenceDoesNotMatch,
        };
        var journeyInstance = await CreateJourneyInstance(
                JourneyNames.RejectNpqTrnRequest,
                state,
                new KeyValuePair<string, object>("supportTaskReference", supportTask.SupportTaskReference));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/reject/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var reasonChoiceSelection = doc.GetElementByTestId("reason-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Single(i => i.IsChecked == true).Value;

        Assert.Equal(state.RejectionReason.ToString(), reasonChoiceSelection);
    }

    [Fact]
    public async Task Post_NoReasonSelected_RendersError()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync("NPQ");

        var supportTask = await new CreateNpqTrnRequestSupportTaskBuilder(applicationUser.UserId)
            .WithMatches(false)
            .ExecuteAsync(TestData);

        var state = new RejectNpqTrnRequestState();
        var journeyInstance = await CreateJourneyInstance(
                JourneyNames.RejectNpqTrnRequest,
                state,
                new KeyValuePair<string, object>("supportTaskReference", supportTask.SupportTaskReference));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/reject/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "RejectionReason", "Select a reason for rejecting this request");
    }

    [Fact]
    public async Task Post_ReasonSelected_PersistsDataAndRedirects()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync("NPQ");

        var supportTask = await new CreateNpqTrnRequestSupportTaskBuilder(applicationUser.UserId)
            .WithMatches(false)
            .ExecuteAsync(TestData);

        var state = new RejectNpqTrnRequestState();
        var journeyInstance = await CreateJourneyInstance(
                JourneyNames.RejectNpqTrnRequest,
                state,
                new KeyValuePair<string, object>("supportTaskReference", supportTask.SupportTaskReference));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/reject/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "RejectionReason", $"{RejectionReasonOption.EvidenceDoesNotMatch}" }
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(RejectionReasonOption.EvidenceDoesNotMatch, journeyInstance.State.RejectionReason);
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/reject/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }
}

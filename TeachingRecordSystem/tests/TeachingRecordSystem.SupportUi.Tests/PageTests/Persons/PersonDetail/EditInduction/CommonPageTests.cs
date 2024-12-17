using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class CommonPageTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData("edit-induction/status", InductionStatus.Exempt, "edit-induction/exemption-reasons")]
    [InlineData("edit-induction/status", InductionStatus.InProgress, "edit-induction/start-date")]
    [InlineData("edit-induction/status", InductionStatus.Failed, "edit-induction/start-date")]
    [InlineData("edit-induction/status", InductionStatus.FailedInWales, "edit-induction/start-date")]
    [InlineData("edit-induction/status", InductionStatus.Passed, "edit-induction/start-date")]
    [InlineData("edit-induction/status", InductionStatus.RequiredToComplete, "edit-induction/change-reason")]
    [InlineData("edit-induction/exemption-reasons", InductionStatus.Exempt, "edit-induction/change-reason")]
    [InlineData("edit-induction/start-date", InductionStatus.InProgress, "edit-induction/change-reason")]
    [InlineData("edit-induction/start-date", InductionStatus.Failed, "edit-induction/date-completed")]
    [InlineData("edit-induction/start-date", InductionStatus.FailedInWales, "edit-induction/date-completed")]
    [InlineData("edit-induction/start-date", InductionStatus.Passed, "edit-induction/date-completed")]
    [InlineData("edit-induction/date-completed", InductionStatus.Failed, "edit-induction/change-reason")]
    [InlineData("edit-induction/date-completed", InductionStatus.FailedInWales, "edit-induction/change-reason")]
    [InlineData("edit-induction/date-completed", InductionStatus.Passed, "edit-induction/change-reason")]
    [InlineData("edit-induction/change-reason", InductionStatus.Exempt, "edit-induction/check-answers")]
    [InlineData("edit-induction/change-reason", InductionStatus.InProgress, "edit-induction/check-answers")]
    [InlineData("edit-induction/change-reason", InductionStatus.Failed, "edit-induction/check-answers")]
    [InlineData("edit-induction/change-reason", InductionStatus.FailedInWales, "edit-induction/check-answers")]
    [InlineData("edit-induction/change-reason", InductionStatus.Passed, "edit-induction/check-answers")]
    [InlineData("edit-induction/change-reason", InductionStatus.RequiredToComplete, "edit-induction/check-answers")]
    [InlineData("edit-induction/check-answers", InductionStatus.RequiredToComplete, "induction")]
    public async Task Post_RedirectsToExpectedPage(string fromPage, InductionStatus inductionStatus, string expectedNextPageUrl)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(
            p => p
                .WithQts()
                .WithInductionStatus(i => i
                    .WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState()
            {
                Initialized = true,
                InductionStatus = inductionStatus
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/{fromPage}?{journeyInstance.GetUniqueIdQueryParameter()}");
        if (fromPage == "edit-induction/status")
        {
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string?>
            {
                { "InductionStatus", inductionStatus.ToString() }
            });
        }

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        var expectedUrl = expectedNextPageUrl == "induction"
            ? $"/persons/{person.PersonId}/{expectedNextPageUrl}"
            : $"/persons/{person.PersonId}/{expectedNextPageUrl}?{journeyInstance.GetUniqueIdQueryParameter()}";
        Assert.Equal(expectedUrl, location);
    }

    [Theory]
    [InlineData("edit-induction/status")]
    [InlineData("edit-induction/exemption-reasons")]
    [InlineData("edit-induction/start-date")]
    [InlineData("edit-induction/date-completed")]
    [InlineData("edit-induction/change-reason")]
    [InlineData("edit-induction/check-answers")]
    public async Task Cancel_RedirectsToExpectedPage(string fromPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState()
            {
                Initialized = true,
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/{fromPage}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var cancelButton = doc.GetElementByTestId("cancel-button") as IHtmlButtonElement;

        // Act
        var redirectRequest = new HttpRequestMessage(HttpMethod.Post, cancelButton!.FormAction);
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)redirectResponse.StatusCode);
        var location = redirectResponse.Headers.Location?.OriginalString;
        Assert.Equal($"/persons/{person.PersonId}/induction", location);
    }

    [Theory]
    [InlineData("edit-induction/status", InductionJourneyPage.Status)]
    [InlineData("edit-induction/exemption-reasons", InductionJourneyPage.ExemptionReason)]
    [InlineData("edit-induction/start-date", InductionJourneyPage.StartDate)]
    [InlineData("edit-induction/date-completed", InductionJourneyPage.CompletedDate)]
    public async Task Post_PersistsStartPage(string page, InductionJourneyPage pageName)
    {
        // Arrange
        InductionStatus inductionStatus = InductionStatus.Passed;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState()
            {
                Initialized = true,
                InductionStatus = inductionStatus
            });
        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/{page}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(pageName, journeyInstance.State.JourneyStartPage);
    }

    [Theory]
    [InlineData("edit-induction/status", InductionStatus.Exempt)]
    [InlineData("edit-induction/status", InductionStatus.InProgress)]
    [InlineData("edit-induction/status", InductionStatus.Failed)]
    [InlineData("edit-induction/status", InductionStatus.FailedInWales)]
    [InlineData("edit-induction/status", InductionStatus.Passed)]
    [InlineData("edit-induction/status", InductionStatus.RequiredToComplete)]
    [InlineData("edit-induction/exemption-reasons", InductionStatus.Exempt)]
    [InlineData("edit-induction/start-date", InductionStatus.InProgress)]
    [InlineData("edit-induction/start-date", InductionStatus.Failed)]
    [InlineData("edit-induction/start-date", InductionStatus.FailedInWales)]
    [InlineData("edit-induction/start-date", InductionStatus.Passed)]
    [InlineData("edit-induction/date-completed", InductionStatus.Failed)]
    [InlineData("edit-induction/date-completed", InductionStatus.FailedInWales)]
    [InlineData("edit-induction/date-completed", InductionStatus.Passed)]
    [InlineData("edit-induction/change-reason", InductionStatus.Exempt)]
    [InlineData("edit-induction/change-reason", InductionStatus.InProgress)]
    [InlineData("edit-induction/change-reason", InductionStatus.Failed)]
    [InlineData("edit-induction/change-reason", InductionStatus.FailedInWales)]
    [InlineData("edit-induction/change-reason", InductionStatus.Passed)]
    [InlineData("edit-induction/change-reason", InductionStatus.RequiredToComplete)]
    public async Task FollowingRedirect_BacklinkContainsExpected(string fromPage, InductionStatus inductionStatus)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState()
            {
                Initialized = true,
                InductionStatus = inductionStatus
            });
        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/{fromPage}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        var backlink = redirectDoc.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.Contains(fromPage, backlink!.Href);
    }

    private Task<JourneyInstance<EditInductionState>> CreateJourneyInstanceAsync(Guid personId, EditInductionState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditInduction,
            state ?? new EditInductionState(),
            new KeyValuePair<string, object>("personId", personId));
}

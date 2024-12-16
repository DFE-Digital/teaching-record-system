using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class CommonPageTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData("edit-induction/status", InductionStatus.Exempt, "exemption-reasons")]
    [InlineData("edit-induction/status", InductionStatus.InProgress, "start-date")]
    [InlineData("edit-induction/status", InductionStatus.Failed, "start-date")]
    [InlineData("edit-induction/status", InductionStatus.FailedInWales, "start-date")]
    [InlineData("edit-induction/status", InductionStatus.Passed, "start-date")]
    [InlineData("edit-induction/status", InductionStatus.RequiredToComplete, "change-reason")]
    [InlineData("edit-induction/exemption-reasons", InductionStatus.Exempt, "change-reason")]
    [InlineData("edit-induction/start-date", InductionStatus.InProgress, "change-reason")]
    [InlineData("edit-induction/start-date", InductionStatus.Failed, "date-completed")]
    [InlineData("edit-induction/start-date", InductionStatus.FailedInWales, "date-completed")]
    [InlineData("edit-induction/start-date", InductionStatus.Passed, "date-completed")]
    [InlineData("edit-induction/date-completed", InductionStatus.Failed, "change-reason")]
    [InlineData("edit-induction/date-completed", InductionStatus.FailedInWales, "change-reason")]
    [InlineData("edit-induction/date-completed", InductionStatus.Passed, "change-reason")]
    [InlineData("edit-induction/change-reason", InductionStatus.Exempt, "check-answers")]
    [InlineData("edit-induction/change-reason", InductionStatus.InProgress, "check-answers")]
    [InlineData("edit-induction/change-reason", InductionStatus.Failed, "check-answers")]
    [InlineData("edit-induction/change-reason", InductionStatus.FailedInWales, "check-answers")]
    [InlineData("edit-induction/change-reason", InductionStatus.Passed, "check-answers")]
    [InlineData("edit-induction/change-reason", InductionStatus.RequiredToComplete, "check-answers")]
    public async Task Post_RedirectsToExpectedPage(string fromPage, InductionStatus inductionStatus, string expectedNextPageUrl)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(
            personBuilder => personBuilder
                .WithInductionStatus(inductionBuilder => inductionBuilder
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
        Assert.Contains(expectedNextPageUrl, location);
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
        var person = await TestData.CreatePersonAsync();

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

    private Task<JourneyInstance<EditInductionState>> CreateJourneyInstanceAsync(Guid personId, EditInductionState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditInduction,
            state ?? new EditInductionState(),
            new KeyValuePair<string, object>("personId", personId));
}

using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;


public class EditInductionStatusTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Buttons_PostToExpectedPage()
    {
        // Arrange
        InductionStatus inductionStatus = InductionStatus.Passed;
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState()
            {
                Initialized = true,
                InductionStatus = inductionStatus
            });
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var form = doc.GetElementByTestId("submit-form") as IHtmlFormElement;
        Assert.NotNull(form);
        Assert.Contains($"/persons/{person.PersonId}/edit-induction/status", form.Action);
        var buttons = form.GetElementsByTagName("button").Select(button => button as IHtmlButtonElement);
        Assert.Equal(2, buttons.Count());
        Assert.Equal($"/persons/{person.PersonId}/induction", buttons.ElementAt(1)!.FormAction);
    }

    [Theory]
    [InlineData(InductionStatus.Exempt, "Select exemption reason")]
    [InlineData(InductionStatus.InProgress, "Start date")]
    [InlineData(InductionStatus.Failed, "Start date")]
    [InlineData(InductionStatus.FailedInWales, "Start date")]
    [InlineData(InductionStatus.Passed, "Start date")]
    [InlineData(InductionStatus.RequiredToComplete, "Change reason")]
    public async Task Post_RedirectsToExpectedPage(InductionStatus inductionStatus, string expectedNextPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState()
            {
                Initialized = true,
                InductionStatus = inductionStatus
            });
        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();

        // Assert
        var title = redirectDoc.GetElementById("page-title")!.TextContent;
        Assert.Contains(expectedNextPage, title);
    }

    [Fact]
    public void BackLink_LinksToExpectedPage()
    {
        throw new NotImplementedException();
    }

    private Task<JourneyInstance<EditInductionState>> CreateJourneyInstanceAsync(Guid personId, EditInductionState? state = null) =>
    CreateJourneyInstance(
        JourneyNames.EditInduction,
        state ?? new EditInductionState(),
        new KeyValuePair<string, object>("personId", personId));
}

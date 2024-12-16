using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;


public class EditInductionStatusTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task ContinueAndCancelButtons_ExistOnPage()
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
        Assert.Equal("Continue", buttons.ElementAt(0)!.TextContent);
        Assert.Equal("Cancel and return to record", buttons.ElementAt(1)!.TextContent);
    }

    private Task<JourneyInstance<EditInductionState>> CreateJourneyInstanceAsync(Guid personId, EditInductionState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditInduction,
            state ?? new EditInductionState(),
            new KeyValuePair<string, object>("personId", personId));
}

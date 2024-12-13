using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class EditStartDateTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData(InductionStatus.Passed)]
    public async Task Buttons_PostToExpectedPage(InductionStatus inductionStatus)
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
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var form = doc.GetElementByTestId("submit-form") as IHtmlFormElement;
        Assert.NotNull(form);
        Assert.Contains($"/persons/{person.PersonId}/edit-induction/start-date", form.Action);
        var buttons = form.GetElementsByTagName("button").Select(button => button as IHtmlButtonElement);
        Assert.Equal(2, buttons.Count());
        Assert.Equal($"/persons/{person.PersonId}/induction", buttons.ElementAt(1)!.FormAction);
    }

    private Task<JourneyInstance<EditInductionState>> CreateJourneyInstanceAsync(Guid personId, EditInductionState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditInduction,
            state ?? new EditInductionState(),
            new KeyValuePair<string, object>("personId", personId));
}

using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDateOfBirth;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditDateOfBirth;

public class ConfirmTests : TestBase
{
    public ConfirmTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/edit-date-of-birth/confirm");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_StateHasNoDateOfBirth_RedirectsToIndex()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var editDateOfBirthState = new EditDateOfBirthState();

        var journeyInstance = await CreateJourneyInstance(
           person.PersonId,
           editDateOfBirthState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-date-of-birth/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}/edit-date-of-birth?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequest_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var newDateOfBirth = TestData.GenerateChangedDateOfBirth(person.DateOfBirth);
        var journeyInstance = await CreateJourneyInstance(
            person.PersonId,
            new EditDateOfBirthState()
            {
                Initialized = true,
                DateOfBirth = newDateOfBirth,
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-date-of-birth/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal(person.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), doc.GetElementByTestId("current-value")!.TextContent);
        Assert.Equal(newDateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), doc.GetElementByTestId("new-value")!.TextContent);
    }

    [Fact]
    public async Task Post_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstance(personId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{personId}/edit-date-of-birth/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_ValidRequest_UpdatesDateOfBirthAndCompletesJourney()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var newDateOfBirth = TestData.GenerateChangedDateOfBirth(person.DateOfBirth);
        var journeyInstance = await CreateJourneyInstance(
            person.PersonId,
            new EditDateOfBirthState()
            {
                Initialized = true,
                DateOfBirth = newDateOfBirth,
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-date-of-birth/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var updatedContact = XrmFakedContext.GetEntityById<Contact>(person.ContactId);
        Assert.Equal(newDateOfBirth, updatedContact.BirthDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: false));

        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}", response.Headers.Location?.OriginalString);
        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Record has been updated");

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    private async Task<JourneyInstance<EditDateOfBirthState>> CreateJourneyInstance(Guid personId, EditDateOfBirthState? state = null) =>
    await CreateJourneyInstance(
        JourneyNames.EditDateOfBirth,
        state ?? new EditDateOfBirthState(),
        new KeyValuePair<string, object>("personId", personId));
}

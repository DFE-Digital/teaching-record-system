using FormFlow;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditName;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditName;

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

        var journeyInstance = await CreateJourneyInstance(personId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/edit-name/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData("NewFirstName", null, null)]
    [InlineData(null, "NewMiddleName", null)]
    [InlineData(null, null, "NewLastName")]
    public async Task Get_StateHasMissingRequiredData_RedirectsToIndex(string? firstName, string? middleName, string? lastName)
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var editNameState = new EditNameState();
        if (firstName is not null)
        {
            editNameState.FirstName = firstName;
        }

        if (middleName is not null)
        {
            editNameState.MiddleName = middleName;
        }

        if (lastName is not null)
        {
            editNameState.LastName = lastName;
        }

        var journeyInstance = await CreateJourneyInstance(
            person.PersonId,
            editNameState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-name/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}/edit-name?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequest_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var newFirstName = TestData.GenerateChangedFirstName(person.FirstName);
        var newMiddleName = TestData.GenerateChangedMiddleName(person.MiddleName);
        var newLastName = TestData.GenerateChangedLastName(person.LastName);
        var journeyInstance = await CreateJourneyInstance(
            person.PersonId,
            new EditNameState()
            {
                Initialized = true,
                FirstName = newFirstName,
                MiddleName = newMiddleName,
                LastName = newLastName,
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-name/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        Assert.Equal($"{person.FirstName} {person.MiddleName} {person.LastName}", doc.GetElementByTestId("current-value")!.TextContent);
        Assert.Equal($"{newFirstName} {newMiddleName} {newLastName}", doc.GetElementByTestId("new-value")!.TextContent);
    }

    [Fact]
    public async Task Post_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstance(personId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{personId}/edit-name/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_ValidRequest_UpdatesNameAndCompletesJourney()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var newFirstName = TestData.GenerateChangedFirstName(person.FirstName);
        var newMiddleName = TestData.GenerateChangedMiddleName(person.MiddleName);
        var newLastName = TestData.GenerateChangedLastName(person.LastName);
        var journeyInstance = await CreateJourneyInstance(
            person.PersonId,
            new EditNameState()
            {
                Initialized = true,
                FirstName = newFirstName,
                MiddleName = newMiddleName,
                LastName = newLastName,
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-name/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var updatedContact = XrmFakedContext.GetEntityById<Contact>(person.ContactId);
        Assert.Equal(newFirstName, updatedContact.FirstName);
        Assert.Equal(newMiddleName, updatedContact.MiddleName);
        Assert.Equal(newLastName, updatedContact.LastName);

        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}", response.Headers.Location?.OriginalString);

        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Record has been updated");

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }


    private async Task<JourneyInstance<EditNameState>> CreateJourneyInstance(Guid personId, EditNameState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditName,
            state ?? new EditNameState(),
            new KeyValuePair<string, object>("personId", personId));
}

using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditName;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditName;

public class IndexTests : TestBase
{
    public IndexTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/edit-name");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestForNewJourney_PopulatesModelFromDqt()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-name");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var doc = await redirectResponse.GetDocumentAsync();
        Assert.Equal(person.FirstName, doc.GetElementById("FirstName")!.GetAttribute("value"));
        Assert.Equal(person.MiddleName, doc.GetElementById("MiddleName")!.GetAttribute("value"));
        Assert.Equal(person.LastName, doc.GetElementById("LastName")!.GetAttribute("value"));
    }

    [Fact]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var newFirstName = TestData.GenerateChangedFirstName(person.FirstName);
        var newMiddleName = TestData.GenerateChangedMiddleName(person.MiddleName);
        var newLastName = TestData.GenerateChangedLastName(person.LastName);
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditNameState()
            {
                Initialized = true,
                FirstName = newFirstName,
                MiddleName = newMiddleName,
                LastName = newLastName,
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-name?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(newFirstName, doc.GetElementById("FirstName")!.GetAttribute("value"));
        Assert.Equal(newMiddleName, doc.GetElementById("MiddleName")!.GetAttribute("value"));
        Assert.Equal(newLastName, doc.GetElementById("LastName")!.GetAttribute("value"));
    }

    [Theory]
    [MemberData(nameof(InvalidNamesData))]
    public async Task Post_WithInvalidData_ReturnsExpectedErrors(
        string firstName,
        string middleName,
        string lastName,
        string expectedErrorElementId,
        string expectedErrorMessage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-name?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string?>
            {
                { "FirstName", firstName },
                { "MiddleName", middleName },
                { "LastName", lastName },
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, expectedErrorElementId, expectedErrorMessage);
    }

    [Fact]
    public async Task Post_WithValidData_RedirectsToConfirmPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var newFirstName = TestData.GenerateChangedFirstName(person.FirstName);
        var newMiddleName = TestData.GenerateChangedMiddleName(person.MiddleName);
        var newLastName = TestData.GenerateChangedLastName(person.LastName);
        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-name?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string?>
            {
                { "FirstName", newFirstName },
                { "MiddleName", newMiddleName },
                { "LastName", newLastName },
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}/edit-name/confirm?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    public static TheoryData<string, string, string, string, string> InvalidNamesData { get; } = new()
    {
        {
            "Joe",
            "",
            "",
            "LastName",
            "Enter a last name"
        },
        {
            "",
            "",
            "Bloggs",
            "FirstName",
            "Enter a first name"
        },
        {
            new string('x', 101),
            "",
            "Bloggs",
            "FirstName",
            "First name must be 100 characters or less"
        },
        {
            "Joe",
            "",
            new string('x', 101),
            "LastName",
            "Last name must be 100 characters or less"
        },
        {
            "Joe",
            new string('x', 101),
            "Bloggs",
            "MiddleName",
            "Middle name must be 100 characters or less"
        }
    };

    private async Task<JourneyInstance<EditNameState>> CreateJourneyInstanceAsync(Guid personId, EditNameState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditName,
            state ?? new EditNameState(),
            new KeyValuePair<string, object>("personId", personId));
}

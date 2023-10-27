using FormFlow;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDateOfBirth;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditDateOfBirth;

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/edit-date-of-birth");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestForNewJourney_PopulatesModelFromDqt()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-date-of-birth");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirect(HttpClient);

        var doc = await redirectResponse.GetDocument();
        Assert.Equal($"{person.DateOfBirth:%d}", doc.GetElementById("DateOfBirth.Day")!.GetAttribute("value"));
        Assert.Equal($"{person.DateOfBirth:%M}", doc.GetElementById("DateOfBirth.Month")!.GetAttribute("value"));
        Assert.Equal($"{person.DateOfBirth:yyyy}", doc.GetElementById("DateOfBirth.Year")!.GetAttribute("value"));
    }

    [Fact]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_PopulatesModelFromJourneyState()
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-date-of-birth?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        Assert.Equal($"{newDateOfBirth:%d}", doc.GetElementById("DateOfBirth.Day")!.GetAttribute("value"));
        Assert.Equal($"{newDateOfBirth:%M}", doc.GetElementById("DateOfBirth.Month")!.GetAttribute("value"));
        Assert.Equal($"{newDateOfBirth:yyyy}", doc.GetElementById("DateOfBirth.Year")!.GetAttribute("value"));
    }

    [Theory]
    [MemberData(nameof(InvalidDateOfBirthData))]
    public async Task Post_WithInvalidData_ReturnsError(
        string day,
        string month,
        string year,
        string expectedErrorElementId,
        string expectedErrorMessage)
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var journeyInstance = await CreateJourneyInstance(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-date-of-birth?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string?>
            {
                { "DateOfBirth.Day", day },
                { "DateOfBirth.Month", month },
                { "DateOfBirth.Year", year },
            }),
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, expectedErrorElementId, expectedErrorMessage);
    }

    [Fact]
    public async Task Post_WithValidData_RedirectsToConfirmPage()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var newDateOfBirth = TestData.GenerateChangedDateOfBirth(person.DateOfBirth);
        var journeyInstance = await CreateJourneyInstance(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-date-of-birth?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string?>
            {
                { "DateOfBirth.Day", $"{newDateOfBirth:%d}" },
                { "DateOfBirth.Month", $"{newDateOfBirth:%M}" },
                { "DateOfBirth.Year", $"{newDateOfBirth:yyyy}" },
            }),
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}/edit-date-of-birth/confirm?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    public static TheoryData<string, string, string, string, string> InvalidDateOfBirthData { get; } = new()
    {
        {
            "",
            "",
            "",
            "DateOfBirth",
            "Enter a date of birth"
        },
        {
            "",
            "10",
            "1969",
            "DateOfBirth",
            "Date of birth must include a day"
        },
        {
            "10",
            "",
            "1969",
            "DateOfBirth",
            "Date of birth must include a month"
        },
        {
            "10",
            "10",
            "",
            "DateOfBirth",
            "Date of birth must include a year"
        },
        {
            "",
            "",
            "1969",
            "DateOfBirth",
            "Date of birth must include a day and month"
        },
        {
            "",
            "10",
            "",
            "DateOfBirth",
            "Date of birth must include a day and year"
        },
        {
            "10",
            "",
            "",
            "DateOfBirth",
            "Date of birth must include a month and year"
        },
        {
            "32",
            "10",
            "1969",
            "DateOfBirth",
            "Date of birth must be a real date"
        },
        {
            "Blah",
            "Blah",
            "Blah",
            "DateOfBirth",
            "Date of birth must be a real date"
        },
    };

    private async Task<JourneyInstance<EditDateOfBirthState>> CreateJourneyInstance(Guid personId, EditDateOfBirthState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditDateOfBirth,
            state ?? new EditDateOfBirthState(),
            new KeyValuePair<string, object>("personId", personId));
}

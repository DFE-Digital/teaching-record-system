using TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.AddAlert;

public class StartDateTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstance(personId, new AddAlertState
        {
            AlertTypeId = Guid.NewGuid(),
            Details = "Details"
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/start-date?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_MissingDataInJourneyState_RedirectsToDetailsPage()
    {
        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState
        {
            AlertTypeId = Guid.NewGuid(),
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/add/details?personId={person.PersonId}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_WithPersonIdForValidPerson_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState
        {
            AlertTypeId = Guid.NewGuid(),
            Details = "Details"
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var startDate = new DateOnly(2021, 1, 1);

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState
        {
            AlertTypeId = Guid.NewGuid(),
            Details = "Details",
            StartDate = startDate
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal($"{startDate:%d}", doc.GetElementById("StartDate.Day")?.GetAttribute("value"));
        Assert.Equal($"{startDate:%M}", doc.GetElementById("StartDate.Month")?.GetAttribute("value"));
        Assert.Equal($"{startDate:yyyy}", doc.GetElementById("StartDate.Year")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Post_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstance(personId, new AddAlertState
        {
            AlertTypeId = Guid.NewGuid(),
            Details = "Details"
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/start-date?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["StartDate.Day"] = "1",
                ["StartDate.Month"] = "1",
                ["StartDate.Year"] = "2021"
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithMissingDataInJourneyState_RedirectsToDetailsPage()
    {
        // Arrange
        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState
        {
            AlertTypeId = Guid.NewGuid()
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["StartDate.Day"] = "1",
                ["StartDate.Month"] = "1",
                ["StartDate.Year"] = "2021"
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/add/details?personId={person.PersonId}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_WhenNoStartDateIsEntered_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState
        {
            AlertTypeId = Guid.NewGuid(),
            Details = "Details"
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "StartDate", "Enter a start date");
    }

    [Fact]
    public async Task Post_WhenStartDateIsInTheFuture_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var futureDate = Clock.Today.AddDays(2);

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState
        {
            AlertTypeId = Guid.NewGuid(),
            Details = "Details"
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["StartDate.Day"] = $"{futureDate.Day}",
                ["StartDate.Month"] = $"{futureDate.Month}",
                ["StartDate.Year"] = $"{futureDate.Year}"
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "StartDate", "Start date cannot be in the future");
    }

    [Fact]
    public async Task Post_WithValidInput_RedirectsToReasonPage()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var startDate = Clock.Today;

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState
        {
            AlertTypeId = Guid.NewGuid(),
            Details = "Details"
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["StartDate.Day"] = $"{startDate.Day}",
                ["StartDate.Month"] = $"{startDate.Month}",
                ["StartDate.Year"] = $"{startDate.Year}"
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/add/reason?personId={person.PersonId}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState
        {
            AlertTypeId = Guid.NewGuid(),
            Details = "Details"
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/start-date/cancel?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    private async Task<JourneyInstance<AddAlertState>> CreateJourneyInstance(Guid personId, AddAlertState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.AddAlert,
            state ?? new AddAlertState(),
            new KeyValuePair<string, object>("personId", personId));
}

using TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.AddAlert;

public class StartDateTests : TestBase
{
    public StartDateTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentUser(TestUsers.AllAlertsWriter);
    }

    [Fact]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.NoRoles);

        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(personId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/start-date?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_MissingDataInJourneyState_RedirectsToLinkPage()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/add/link?personId={person.PersonId}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_WithPersonIdForValidPerson_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false
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
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();
        var startDate = new DateOnly(2021, 1, 1);

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false,
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
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.NoRoles);

        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();
        var startDate = Clock.Today;

        var journeyInstance = await CreateJourneyInstance(personId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/start-date?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "StartDate.Day", startDate.Day },
                { "StartDate.Month", startDate.Month },
                { "StartDate.Year", startDate.Year }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithMissingDataInJourneyState_RedirectsToLinkPage()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();
        var startDate = Clock.Today;

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details"
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "StartDate.Day", startDate.Day },
                { "StartDate.Month", startDate.Month },
                { "StartDate.Year", startDate.Year }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/add/link?personId={person.PersonId}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_WhenNoStartDateIsEntered_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false
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
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();
        var startDate = Clock.Today.AddDays(2);

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "StartDate.Day", startDate.Day },
                { "StartDate.Month", startDate.Month },
                { "StartDate.Year", startDate.Year }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "StartDate", "Start date cannot be in the future");
    }

    [Fact]
    public async Task Post_WithValidInput_UpdatesStateAndRedirectsToReasonPage()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();
        var startDate = Clock.Today;

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "StartDate.Day", startDate.Day },
                { "StartDate.Month", startDate.Month },
                { "StartDate.Year", startDate.Year }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/add/reason?personId={person.PersonId}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(startDate, journeyInstance.State.StartDate);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false
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

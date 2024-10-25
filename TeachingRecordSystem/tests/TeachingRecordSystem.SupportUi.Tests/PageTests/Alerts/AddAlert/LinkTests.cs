using TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.AddAlert;

public class LinkTests : TestBase
{
    public LinkTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsReadWrite, UserRoles.DbsAlertsReadWrite));
    }

    [Fact]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(roles: []));

        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/link?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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
            AlertTypeName = alertType.Name
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/link?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_MissingDataInJourneyState_Redirects()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/link?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details"
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/link?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var link = TestData.GenerateUrl();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            Link = link
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/link?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal(link, doc.GetElementById("Link")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(roles: []));

        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details"
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/link?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var journeyInstance = await CreateJourneyInstance(personId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details"
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/link?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithMissingDataInJourneyState_Redirects()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/link?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/add/details?personId={person.PersonId}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_AddLinkNotAnswered_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details"
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/link?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "AddLink", "Select yes if you want to add a link to a panel outcome");
    }

    [Fact]
    public async Task Post_WithInvalidLinkUrl_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details"
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/link?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "AddLink", bool.TrueString },
                { "Link", "badurl" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "Link", "Enter a valid URL");
    }

    [Fact]
    public async Task Post_WithLink_UpdatesStateAndRedirectsToStartDatePage()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details"
        });

        var link = "http://example.com";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/link?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "AddLink", bool.TrueString },
                { "Link", link }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/add/start-date?personId={person.PersonId}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.State.AddLink);
        Assert.Equal(link, journeyInstance.State.Link);
    }

    [Fact]
    public async Task Post_WithNoLink_UpdatesStateAndRedirectsToStartDatePage()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details"
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/link?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "AddLink", bool.FalseString }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/add/start-date?personId={person.PersonId}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.False(journeyInstance.State.AddLink);
        Assert.Null(journeyInstance.State.Link);
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
            Details = "Details"
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/link/cancel?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.Details;

public class IndexTests(HostFixture hostFixture) : DetailsTestBase(hostFixture)
{
    private const string ThisStep = JourneySteps.Index;

    [Before(Test)]
    public async Task SetUser() => SetCurrentUser(await TestData.CreateUserAsync(role: UserRoles.AlertsManagerTraDbs));

    [Test]
    [RolesWithoutAlertWritePermissionData]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_WithClosedAlert_ReturnsBadRequest()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_ValidRequestWithUninitializedJourneyState_PopulatesModelFromDatabase()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(alert.Details, doc.GetElementById("Details")?.TrimmedText());
    }

    [Test]
    public async Task Get_ValidRequestWithInitializedJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(ThisStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(journeyInstance.State.Details, doc.GetElementById("Details")?.TrimmedText());
    }

    [Test]
    [RolesWithoutAlertWritePermissionData]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);
        var newDetails = "New details";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(newDetails)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_WithClosedAlert_ReturnsBadRequest()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_WithUnchangedDetails_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);
        var newDetails = alert.Details;

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(newDetails)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Details", "Enter changed details");
    }

    [Test]
    public async Task Post_WithEmptyDetails_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);
        var newDetails = string.Empty;

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(newDetails)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Details", "Enter details");
    }

    [Test]
    public async Task Post_WhenChangedDetailsEntered_UpdatesStateAndRedirectsToChangeReasonPage()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);
        var newDetails = "New details";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(newDetails)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/detailsreason", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(newDetails, journeyInstance.State.Details);
    }

    [Test]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/details/cancel?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/alerts", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    [Test]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(httpMethod, $"/alerts/{alert.AlertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    private static FormUrlEncodedContentBuilder CreatePostContent(string? newDetails)
    {
        var builder = new FormUrlEncodedContentBuilder();
        if (newDetails is not null)
        {
            builder.Add("Details", newDetails);
        }

        return builder;
    }
}

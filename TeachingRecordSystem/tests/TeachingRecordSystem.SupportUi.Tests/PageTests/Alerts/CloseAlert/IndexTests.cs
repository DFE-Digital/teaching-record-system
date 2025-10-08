namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.CloseAlert;

public class IndexTests(HostFixture hostFixture) : CloseAlertTestBase(hostFixture)
{
    private const string PreviousStep = JourneySteps.New;
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
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/close?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/close?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/close?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_ValidRequestWithUninitializedJourneyState_ReturnsOK()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/close?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_ValidRequestWithInitializedJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(ThisStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/close?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal($"{journeyInstance.State.EndDate:%d}", doc.GetElementById("EndDate.Day")?.GetAttribute("value"));
        Assert.Equal($"{journeyInstance.State.EndDate:%M}", doc.GetElementById("EndDate.Month")?.GetAttribute("value"));
        Assert.Equal($"{journeyInstance.State.EndDate:yyyy}", doc.GetElementById("EndDate.Year")?.GetAttribute("value"));
    }

    [Test]
    [RolesWithoutAlertWritePermissionData]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(JourneySteps.New, alert);
        var endDate = alert.StartDate!.Value.AddDays(5);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/close?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(endDate)
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/close?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/close?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_WhenNoEndDateIsEntered_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/close?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "EndDate", "Enter an end date");
    }

    [Test]
    public async Task Post_WhenEndDateIsInTheFuture_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);
        var futureDate = Clock.Today.AddDays(2);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/close?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(futureDate)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "EndDate", "End date cannot be in the future");
    }

    [Test]
    public async Task Post_WhenEndDateIsBeforeStartDate_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);
        var newEndDate = alert.StartDate!.Value.AddDays(-2);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/close?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(newEndDate)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "EndDate", "End date must be after the start date");
    }

    [Test]
    public async Task Post_WhenEndDateIsEntered_UpdatesStateAndRedirectsToChangeReasonPage()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);
        var newEndDate = alert.StartDate!.Value.AddDays(2);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/close?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(newEndDate)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/closereason", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(newEndDate, journeyInstance.State.EndDate);
    }

    [Test]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/close/cancel?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(httpMethod, $"/alerts/{alert.AlertId}/close?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    private static FormUrlEncodedContentBuilder CreatePostContent(DateOnly? endDate)
    {
        var builder = new FormUrlEncodedContentBuilder();

        if (endDate is not null)
        {
            builder.Add("EndDate.Day", $"{endDate:%d}");
            builder.Add("EndDate.Month", $"{endDate:%M}");
            builder.Add("EndDate.Year", $"{endDate:yyyy}");
        }

        return builder;
    }
}

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.StartDate;

public class IndexTests(HostFixture hostFixture) : StartDateTestBase(hostFixture), IAsyncLifetime
{
    private const string PreviousStep = JourneySteps.New;
    private const string ThisStep = JourneySteps.Index;

    async ValueTask IAsyncLifetime.InitializeAsync() => SetCurrentUser(await TestData.CreateUserAsync(role: UserRoles.AlertsManagerTraDbs));

    ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;

    [Theory]
    [RolesWithoutAlertWritePermissionData]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_AlertDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_AlertIsClosed_ReturnsBadRequest()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithUninitializedJourneyState_PopulatesModelFromDatabase()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal($"{alert.StartDate:%d}", doc.GetElementById("StartDate.Day")?.GetAttribute("value"));
        Assert.Equal($"{alert.StartDate:%M}", doc.GetElementById("StartDate.Month")?.GetAttribute("value"));
        Assert.Equal($"{alert.StartDate:yyyy}", doc.GetElementById("StartDate.Year")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Get_ValidRequestWithInitializedJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(ThisStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal($"{journeyInstance.State.StartDate:%d}", doc.GetElementById("StartDate.Day")?.GetAttribute("value"));
        Assert.Equal($"{journeyInstance.State.StartDate:%M}", doc.GetElementById("StartDate.Month")?.GetAttribute("value"));
        Assert.Equal($"{journeyInstance.State.StartDate:yyyy}", doc.GetElementById("StartDate.Year")?.GetAttribute("value"));
    }

    [Theory]
    [RolesWithoutAlertWritePermissionData]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var (person, alert) = await CreatePersonWithOpenAlert();
        var newStartDate = alert.StartDate!.Value.AddDays(-18);
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(newStartDate)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_AlertDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(newStartDate: TestData.GenerateDate())
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_AlertIsClosed_ReturnsBadRequest()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(newStartDate: alert.StartDate!.Value.AddDays(1))
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_NoStartDateIsEntered_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(newStartDate: null)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "StartDate", "Enter a start date");
    }

    [Fact]
    public async Task Post_StartDateIsInTheFuture_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(newStartDate: Clock.Today.AddDays(1))
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "StartDate", "Start date cannot be in the future");
    }

    [Fact]
    public async Task Post_StartDateIsUnchanged_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(newStartDate: alert.StartDate)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "StartDate", "Enter a different start date");
    }

    [Fact]
    public async Task Post_ValidStartDateIsEntered_UpdatesStateAndRedirectsToChangeReasonPage()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);
        var newStartDate = alert.StartDate!.Value.AddDays(1);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(newStartDate: newStartDate)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/start-date/reason", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(newStartDate, journeyInstance.State.StartDate);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/start-date/cancel?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/alerts", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(httpMethod, $"/alerts/{alert.AlertId}/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    private static FormUrlEncodedContentBuilder CreatePostContent(DateOnly? newStartDate)
    {
        var builder = new FormUrlEncodedContentBuilder();

        if (newStartDate is DateOnly date)
        {
            builder.Add("StartDate.Day", date.Day);
            builder.Add("StartDate.Month", date.Month);
            builder.Add("StartDate.Year", date.Year);
        }

        return builder;
    }
}

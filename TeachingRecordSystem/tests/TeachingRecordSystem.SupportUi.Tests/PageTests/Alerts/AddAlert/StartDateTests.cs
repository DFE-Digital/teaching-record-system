namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.AddAlert;

public class StartDateTests(HostFixture hostFixture) : AddAlertTestBase(hostFixture), IAsyncLifetime
{
    private const string PreviousStep = JourneyStepNames.Link;
    private const string ThisStep = JourneyStepNames.StartDate;

    async ValueTask IAsyncLifetime.InitializeAsync() => SetCurrentUser(await TestData.CreateUserAsync(role: UserRoles.AlertsManagerTraDbs));

    ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;

    [Theory]
    [RolesWithoutAlertWritePermissionData]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, person.PersonId);

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
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, personId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/start-date?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_MissingDataInJourneyState_RedirectsToStartPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(JourneyStepNames.Details, person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(GetStepUrl(PreviousStep, person.PersonId, journeyInstance.InstanceId), response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_WithPersonIdForValidPerson_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, person.PersonId);

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
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(ThisStep, person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(ThisStep, person.PersonId);

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
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(ThisStep, personId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/start-date?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithMissingDataInJourneyState_Redirects()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForIncompleteStepAsync(PreviousStep, person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(GetStepUrl(PreviousStep, person.PersonId, journeyInstance.InstanceId), response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_WhenNoStartDateIsEntered_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "StartDate", "Enter a start date");
    }

    [Fact]
    public async Task Post_WhenStartDateIsInTheFuture_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, person.PersonId);
        var startDate = Clock.Today.AddDays(2);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(startDate)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "StartDate", "Start date cannot be in the future");
    }

    [Fact]
    public async Task Post_WithValidInput_UpdatesStateAndRedirectsToReasonPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, person.PersonId);
        var startDate = Clock.Today;

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(startDate)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(GetStepUrl(JourneyStepNames.Reason, person.PersonId, journeyInstance.InstanceId), response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(startDate, journeyInstance.State.StartDate);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/start-date?handler=cancel&personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, person.PersonId);

        var request = new HttpRequestMessage(httpMethod, $"/alerts/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    private static FormUrlEncodedContentBuilder CreatePostContent(DateOnly? startDate)
    {
        var builder = new FormUrlEncodedContentBuilder();

        if (startDate is DateOnly date)
        {
            builder.Add("StartDate.Day", date.Day);
            builder.Add("StartDate.Month", date.Month);
            builder.Add("StartDate.Year", date.Year);
        }

        return builder;
    }
}

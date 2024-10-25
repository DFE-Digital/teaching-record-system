using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.StartDate;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.StartDate;

public class IndexTests : TestBase
{
    public IndexTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsReadWrite, UserRoles.DbsAlertsReadWrite));
    }

    [Fact]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(roles: []));

        var databaseStartDate = new DateOnly(2021, 10, 5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate).WithEndDate(null)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithClosedAlert_ReturnsBadRequest()
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var databaseEndDate = new DateOnly(2022, 11, 6);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithUninitializedJourneyState_PopulatesModelFromDatabase()
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate).WithEndDate(null)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal($"{databaseStartDate:%d}", doc.GetElementById("StartDate.Day")?.GetAttribute("value"));
        Assert.Equal($"{databaseStartDate:%M}", doc.GetElementById("StartDate.Month")?.GetAttribute("value"));
        Assert.Equal($"{databaseStartDate:yyyy}", doc.GetElementById("StartDate.Year")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Get_ValidRequestWithInitializedJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var journeyStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate).WithEndDate(null)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, currentStartDate: journeyStartDate);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal($"{journeyStartDate:%d}", doc.GetElementById("StartDate.Day")?.GetAttribute("value"));
        Assert.Equal($"{journeyStartDate:%M}", doc.GetElementById("StartDate.Month")?.GetAttribute("value"));
        Assert.Equal($"{journeyStartDate:yyyy}", doc.GetElementById("StartDate.Year")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(roles: []));

        var databaseStartDate = Clock.Today.AddDays(-20);
        var newStartDate = Clock.Today.AddDays(-18);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate).WithEndDate(null)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, currentStartDate: databaseStartDate);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "StartDate.Day", newStartDate.Day },
                { "StartDate.Month", newStartDate.Month },
                { "StartDate.Year", newStartDate.Year },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithClosedAlert_ReturnsBadRequest()
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var databaseEndDate = new DateOnly(2022, 11, 6);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenNoStartDateIsEntered_ReturnsError()
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate).WithEndDate(null)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, currentStartDate: databaseStartDate);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "StartDate", "Enter a start date");
    }

    [Fact]
    public async Task Post_WhenStartDateIsInTheFuture_ReturnsError()
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var futureDate = Clock.Today.AddDays(2);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate).WithEndDate(null)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, currentStartDate: databaseStartDate);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "StartDate.Day", $"{futureDate:%d}" },
                { "StartDate.Month", $"{futureDate:%M}" },
                { "StartDate.Year", $"{futureDate:yyyy}" },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "StartDate", "Start date cannot be in the future");
    }

    [Fact]
    public async Task Post_WhenStartDateIsUnchanged_ReturnsError()
    {
        // Arrange
        var databaseStartDate = Clock.Today;
        var newStartDate = databaseStartDate;
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate).WithEndDate(null)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, currentStartDate: databaseStartDate);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "StartDate.Day", newStartDate.Day },
                { "StartDate.Month", newStartDate.Month },
                { "StartDate.Year", newStartDate.Year },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "StartDate", "Enter a different start date");
    }

    [Fact]
    public async Task Post_WhenStartDateIsEntered_UpdatesStateAndRedirectsToChangeReasonPage()
    {
        // Arrange
        var databaseStartDate = Clock.Today.AddDays(-20);
        var newStartDate = Clock.Today.AddDays(-18);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate).WithEndDate(null)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, currentStartDate: databaseStartDate);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "StartDate.Day", newStartDate.Day },
                { "StartDate.Month", newStartDate.Month },
                { "StartDate.Year", newStartDate.Year },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/start-date/change-reason", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(newStartDate, journeyInstance.State.StartDate);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate).WithEndDate(null)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, currentStartDate: databaseStartDate);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/start-date/cancel?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/alerts", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    private Task<JourneyInstance<EditAlertStartDateState>> CreateJourneyInstance(Guid alertId, DateOnly currentStartDate) =>
        CreateJourneyInstance(
            alertId,
            new EditAlertStartDateState()
            {
                Initialized = true,
                CurrentStartDate = currentStartDate,
                StartDate = currentStartDate
            });

    private async Task<JourneyInstance<EditAlertStartDateState>> CreateJourneyInstance(Guid alertId, EditAlertStartDateState state) =>
        await CreateJourneyInstance(
            JourneyNames.EditAlertStartDate,
            state,
            new KeyValuePair<string, object>("alertId", alertId));
}

using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.EndDate;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.EndDate;

public class IndexTests : TestBase
{
    public IndexTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsReadWrite, UserRoles.DbsAlertsReadWrite));
    }

    [Fact]
    public async Task Get_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/end-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WhenAlertHasNoEndDateSet_ReturnsBadRequest()
    {
        // Arrange
        var startDate = new DateOnly(2021, 10, 5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/end-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithUninitializedJourneyState_PopulatesModelFromDatabase()
    {
        // Arrange
        var startDate = Clock.Today.AddDays(-50);
        var databaseEndDate = Clock.Today.AddDays(-10);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/end-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal($"{databaseEndDate:%d}", doc.GetElementById("EndDate.Day")?.GetAttribute("value"));
        Assert.Equal($"{databaseEndDate:%M}", doc.GetElementById("EndDate.Month")?.GetAttribute("value"));
        Assert.Equal($"{databaseEndDate:yyyy}", doc.GetElementById("EndDate.Year")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Get_ValidRequestWithInitializedJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var startDate = Clock.Today.AddDays(-50);
        var databaseEndDate = Clock.Today.AddDays(-10);
        var journeyEndDate = Clock.Today.AddDays(-5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertEndDateState()
            {
                Initialized = true,
                EndDate = journeyEndDate
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/end-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal($"{journeyEndDate:%d}", doc.GetElementById("EndDate.Day")?.GetAttribute("value"));
        Assert.Equal($"{journeyEndDate:%M}", doc.GetElementById("EndDate.Month")?.GetAttribute("value"));
        Assert.Equal($"{journeyEndDate:yyyy}", doc.GetElementById("EndDate.Year")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Post_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/end-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenAlertHasNoEndDateSet_ReturnsBadRequest()
    {
        // Arrange
        var startDate = Clock.Today.AddDays(-50);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/end-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenNoEndDateIsEntered_ReturnsError()
    {
        var startDate = Clock.Today.AddDays(-50);
        var databaseEndDate = Clock.Today.AddDays(-10);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/end-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "EndDate", "Enter an end date");
    }

    [Fact]
    public async Task Post_WhenEndDateIsInTheFuture_ReturnsError()
    {
        var startDate = Clock.Today.AddDays(-50);
        var databaseEndDate = Clock.Today.AddDays(-10);
        var futureDate = Clock.Today.AddDays(2);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/end-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "EndDate.Day", $"{futureDate:%d}" },
                { "EndDate.Month", $"{futureDate:%M}" },
                { "EndDate.Year", $"{futureDate:yyyy}" },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "EndDate", "End date cannot be in the future");
    }

    [Fact]
    public async Task Post_WhenEndDateIsBeforeStartDate_ReturnsError()
    {
        var startDate = Clock.Today.AddDays(-50);
        var databaseEndDate = Clock.Today.AddDays(-10);
        var newEndDate = Clock.Today.AddDays(-51);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/end-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "EndDate.Day", $"{newEndDate:%d}" },
                { "EndDate.Month", $"{newEndDate:%M}" },
                { "EndDate.Year", $"{newEndDate:yyyy}" },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "EndDate", "End date must be after the start date");
    }

    [Fact]
    public async Task Post_WhenEndDateIsUnchanged_ReturnsError()
    {
        var startDate = Clock.Today.AddDays(-50);
        var databaseEndDate = Clock.Today.AddDays(-10);
        var newEndDate = databaseEndDate;
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/end-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "EndDate.Day", $"{newEndDate:%d}" },
                { "EndDate.Month", $"{newEndDate:%M}" },
                { "EndDate.Year", $"{newEndDate:yyyy}" },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "EndDate", "Enter a different end date");
    }

    [Fact]
    public async Task Post_WhenEndDateIsEntered_RedirectsToChangeReasonPage()
    {
        var startDate = Clock.Today.AddDays(-50);
        var databaseEndDate = Clock.Today.AddDays(-10);
        var newEndDate = Clock.Today.AddDays(-5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/end-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "EndDate.Day", $"{newEndDate:%d}" },
                { "EndDate.Month", $"{newEndDate:%M}" },
                { "EndDate.Year", $"{newEndDate:yyyy}" },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/end-date/change-reason", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        var startDate = Clock.Today.AddDays(-50);
        var databaseEndDate = Clock.Today.AddDays(-10);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/end-date/cancel?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    private async Task<JourneyInstance<EditAlertEndDateState>> CreateJourneyInstance(Guid alertId, EditAlertEndDateState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditAlertEndDate,
            state ?? new EditAlertEndDateState(),
            new KeyValuePair<string, object>("alertId", alertId));
}

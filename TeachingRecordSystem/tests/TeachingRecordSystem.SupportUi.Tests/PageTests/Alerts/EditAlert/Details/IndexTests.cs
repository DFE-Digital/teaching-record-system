using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Details;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.Details;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.NoRoles);

        var databaseStartDate = new DateOnly(2021, 10, 5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate).WithEndDate(null)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithUninitializedJourneyState_PopulatesModelFromDatabase()
    {
        // Arrange
        var details = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithDetails(details)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal(details, doc.GetElementById("Details")?.TextContent);
    }

    [Fact]
    public async Task Get_ValidRequestWithInitializedJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var databaseDetails = TestData.GenerateLoremIpsum();
        var journeyDetails = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithDetails(databaseDetails)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, currentDetails: journeyDetails);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal(journeyDetails, doc.GetElementById("Details")?.TextContent);
    }

    [Fact]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.NoRoles);

        var databaseDetails = TestData.GenerateLoremIpsum();
        var newDetails = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithDetails(databaseDetails)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Details", newDetails }
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithUnchangedDetails_ReturnsError()
    {
        var details = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithDetails(details)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Details", details }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "Details", "Enter changed details");
    }

    [Fact]
    public async Task Post_WithEmptyDetails_ReturnsError()
    {
        var details = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithDetails(details)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Details", string.Empty }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "Details", "Enter details");
    }

    [Fact]
    public async Task Post_WhenChangedDetailsEntered_UpdatesStateAndRedirectsToChangeReasonPage()
    {
        var databaseDetails = TestData.GenerateLoremIpsum();
        var newDetails = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithDetails(databaseDetails)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Details", newDetails }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/details/change-reason", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(newDetails, journeyInstance.State.Details);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var details = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithDetails(details)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/details/cancel?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/alerts", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    private Task<JourneyInstance<EditAlertDetailsState>> CreateJourneyInstance(Guid alertId, string currentDetails) =>
        CreateJourneyInstance(
            alertId,
            new EditAlertDetailsState()
            {
                Initialized = true,
                CurrentDetails = currentDetails,
                Details = currentDetails
            });

    private async Task<JourneyInstance<EditAlertDetailsState>> CreateJourneyInstance(Guid alertId, EditAlertDetailsState state) =>
        await CreateJourneyInstance(
            JourneyNames.EditAlertDetails,
            state,
            new KeyValuePair<string, object>("alertId", alertId));
}

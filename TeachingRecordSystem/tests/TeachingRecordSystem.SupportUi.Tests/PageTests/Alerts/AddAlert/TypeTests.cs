using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.AddAlert;

public class TypeTests : TestBase
{
    public TypeTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsReadWrite, UserRoles.DbsAlertsReadWrite));
    }

    [Fact]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(roles: []));

        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/type?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var journeyInstance = await CreateJourneyInstance(personId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/type?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithPersonIdForValidPerson_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/type?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_UserHasDbsAlertReadWriteRole_ShowsDbsAlertType()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.DbsAlertsReadWrite));

        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/type?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var alertTypeOptions = doc.GetElementsByName("AlertTypeId").Select(e => new Guid(e.GetAttribute("value")!));
        Assert.Contains(AlertType.DbsAlertTypeId, alertTypeOptions);
    }

    [Fact]
    public async Task Get_UserDoesNotHaveDbsAlertReadWriteRole_DoesNotShowDbsAlertType()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsReadWrite));

        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/type?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var alertTypeOptions = doc.GetElementsByName("AlertTypeId").Select(e => new Guid(e.GetAttribute("value")!));
        Assert.DoesNotContain(AlertType.DbsAlertTypeId, alertTypeOptions);
    }

    [Fact]
    public async Task Get_UserHasAlertsReadWriteRole_ShowsAllNonDbsRoles()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsReadWrite));

        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/type?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var alertTypeOptions = doc.GetElementsByName("AlertTypeId").Select(e => new Guid(e.GetAttribute("value")!));
        var nonDbsAlertTypes = (await TestData.ReferenceDataCache.GetAlertTypes(activeOnly: true)).Where(t => !t.IsDbsAlertType);
        Assert.True(alertTypeOptions.SequenceEqualIgnoringOrder(nonDbsAlertTypes.Select(t => t.AlertTypeId)));
    }

    [Fact]
    public async Task Get_UserDoesNotHaveAlertsReadWriteRole_DoesNotShowAnyNonDbsRoles()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.DbsAlertsReadWrite));

        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/type?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var alertTypeOptions = doc.GetElementsByName("AlertTypeId").Select(e => new Guid(e.GetAttribute("value")!));
        var nonDbsAlertTypes = (await TestData.ReferenceDataCache.GetAlertTypes(activeOnly: true)).Where(t => !t.IsDbsAlertType);
        Assert.True(nonDbsAlertTypes.All(t => !alertTypeOptions.Contains(t.AlertTypeId)));
    }

    [Fact]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeById(Guid.Parse("ed0cd700-3fb2-4db0-9403-ba57126090ed")); // Prohibition by the Secretary of State - misconduct

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/type?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var radioButtons = doc.GetElementsByName("AlertTypeId");
        var selectedRadioButton = radioButtons.Single(r => r.HasAttribute("checked"));
        Assert.Equal(alertType.AlertTypeId.ToString(), selectedRadioButton.GetAttribute("value"));
    }

    [Fact]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(roles: []));

        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/type?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var journeyInstance = await CreateJourneyInstance(personId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/type?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "AlertTypeId", alertType.AlertTypeId }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenAlertTypeHasNotBeenSelected_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/type?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "AlertTypeId", "Select an alert type");
    }

    [Fact]
    public async Task Post_ValidInput_UpdatesStateAndRedirectsToDetailsPage()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeById(Guid.Parse("ed0cd700-3fb2-4db0-9403-ba57126090ed")); // Prohibition by the Secretary of State - misconduct

        var journeyInstance = await CreateJourneyInstance(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/type?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "AlertTypeId", alertType.AlertTypeId }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/alerts/add/details?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(alertType.AlertTypeId, journeyInstance.State.AlertTypeId);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/type/CANCEL?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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

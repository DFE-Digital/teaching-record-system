using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.AddAlert;

public class TypeTests : AddAlertTestBase
{
    private const string PreviousStep = JourneySteps.Index;
    private const string ThisStep = JourneySteps.AlertType;

    public TypeTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsManagerTraDbs));
    }

    [Theory]
    [RolesWithoutAlertWritePermissionData]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role));

        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(person.PersonId);

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
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(personId);

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
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(person.PersonId);

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
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsManagerTraDbs));

        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/type?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var alertTypeOptions = doc.GetElementsByName("AlertTypeId").Select(e => new Guid(e.GetAttribute("value")!));
        Assert.Contains(AlertType.DbsAlertTypeId, alertTypeOptions);
    }

    [Fact]
    public async Task Get_UserDoesNotHaveDbsAlertReadWriteRole_DoesNotShowDbsAlertType()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsManagerTra));

        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/type?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var alertTypeOptions = doc.GetElementsByName("AlertTypeId").Select(e => new Guid(e.GetAttribute("value")!));
        Assert.DoesNotContain(AlertType.DbsAlertTypeId, alertTypeOptions);
    }

    [Fact]
    public async Task Get_UserHasAlertsReadWriteRole_ShowsAllNonDbsRoles()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsManagerTra));

        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/type?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var alertTypeOptions = doc.GetElementsByName("AlertTypeId").Select(e => new Guid(e.GetAttribute("value")!));
        var nonDbsAlertTypes = (await TestData.ReferenceDataCache.GetAlertTypesAsync(activeOnly: true)).Where(t => !t.IsDbsAlertType);
        Assert.True(alertTypeOptions.SequenceEqualIgnoringOrder(nonDbsAlertTypes.Select(t => t.AlertTypeId)));
    }

    [Fact]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(ThisStep, person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/type?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var radioButtons = doc.GetElementsByName("AlertTypeId");
        var selectedRadioButton = radioButtons.Single(r => r.HasAttribute("checked"));
        Assert.Equal(journeyInstance.State.AlertTypeId.ToString(), selectedRadioButton.GetAttribute("value"));
    }

    [Theory]
    [RolesWithoutAlertWritePermissionData]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role));

        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(person.PersonId);

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
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(personId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/type?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenAlertTypeHasNotBeenSelected_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/type?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "AlertTypeId", "Select an alert type");
    }

    [Fact]
    public async Task Post_ValidInput_UpdatesStateAndRedirectsToDetailsPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(person.PersonId);
        var alertType = await GetKnownAlertTypeAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/type?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(alertType.AlertTypeId)
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
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/type/cancel?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    [Theory]
    [MemberData(nameof(HttpMethods), TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(httpMethod, $"/alerts/add/type?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    private static FormUrlEncodedContentBuilder CreatePostContent(Guid? alertTypeId)
    {
        var builder = new FormUrlEncodedContentBuilder();
        if (alertTypeId is not null)
        {
            builder.Add("AlertTypeId", alertTypeId);
        }

        return builder;
    }
}

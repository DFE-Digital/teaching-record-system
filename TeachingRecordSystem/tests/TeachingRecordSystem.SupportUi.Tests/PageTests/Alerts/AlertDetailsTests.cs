using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts;

public class AlertDetailsTests : TestBase
{
    public AlertDetailsTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsReadWrite, UserRoles.DbsAlertsReadWrite));
    }

    [Fact]
    public async Task Get_AlertDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_AlertIsOpen_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b
            .WithAlert(a => a.WithEndDate(null)));

        var alert = person.Alerts.Single();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b
            .WithAlert(a => a.WithStartDate(new(2024, 1, 1)).WithEndDate(new(2024, 10, 10))));

        var alert = person.Alerts.Single();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var h1 = doc.GetElementsByTagName("h1").Single();
        Assert.Equal(alert.AlertType.Name, h1.TextContent);

        Assert.Equal(alert.Details, doc.GetSummaryListValueForKey("Details"));
        Assert.Equal(alert.StartDate?.ToString(UiDefaults.DateOnlyDisplayFormat), doc.GetSummaryListValueForKey("Start date"));
        Assert.Equal(alert.EndDate?.ToString(UiDefaults.DateOnlyDisplayFormat), doc.GetSummaryListValueForKey("End date"));
    }

    [Fact]
    public async Task Get_AlertIsDbsAlertAndUserDoesNotHavePermissionToRead_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(roles: []));

        var person = await TestData.CreatePersonAsync(b => b
            .WithAlert(a => a.WithStartDate(new(2024, 1, 1)).WithEndDate(new(2024, 10, 10)).WithAlertTypeId(AlertType.DbsAlertTypeId)));

        var alert = person.Alerts.Single();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_AlertIsDbsAlertAndUserDoesHavePermissionToRead_ReturnsOk()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.DbsAlertsReadOnly));

        var person = await TestData.CreatePersonAsync(b => b
            .WithAlert(a => a.WithStartDate(new(2024, 1, 1)).WithEndDate(new(2024, 10, 10)).WithAlertTypeId(AlertType.DbsAlertTypeId)));

        var alert = person.Alerts.Single();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_AlertIsDbsAlertAndUserDoesHavePermissionToReadAndWrite_ReturnsOk()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.DbsAlertsReadWrite));

        var person = await TestData.CreatePersonAsync(b => b
            .WithAlert(a => a.WithStartDate(new(2024, 1, 1)).WithEndDate(new(2024, 10, 10)).WithAlertTypeId(AlertType.DbsAlertTypeId)));

        var alert = person.Alerts.Single();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_DbsAlertAndUserDoesNotHaveWritePermission_DoesNotShowChangeLinks()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.DbsAlertsReadOnly));

        var person = await TestData.CreatePersonAsync(b => b
            .WithAlert(a => a.WithStartDate(new(2024, 1, 1)).WithEndDate(new(2024, 10, 10)).WithAlertTypeId(AlertType.DbsAlertTypeId)));

        var alert = person.Alerts.Single();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Empty(doc.GetSummaryListActionsForKey("End date"));
    }

    [Fact]
    public async Task Get_DbsAlertAndUserDoesHaveWritePermission_DoesShowChangeLinks()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.DbsAlertsReadWrite));

        var person = await TestData.CreatePersonAsync(b => b
            .WithAlert(a => a.WithStartDate(new(2024, 1, 1)).WithEndDate(new(2024, 10, 10)).WithAlertTypeId(AlertType.DbsAlertTypeId)));

        var alert = person.Alerts.Single();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotEmpty(doc.GetSummaryListActionsForKey("End date"));
    }

    [Fact]
    public async Task Get_NonDbsAlertAndUserDoesNotHaveWritePermission_DoesNotShowChangeLinks()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(roles: []));

        var alertType = (await TestData.ReferenceDataCache.GetAlertTypesAsync(activeOnly: true)).RandomOneExcept(at => at.AlertTypeId == AlertType.DbsAlertTypeId);
        var person = await TestData.CreatePersonAsync(b => b
            .WithAlert(a => a.WithStartDate(new(2024, 1, 1)).WithEndDate(new(2024, 10, 10)).WithAlertTypeId(alertType.AlertTypeId)));

        var alert = person.Alerts.Single();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Empty(doc.GetSummaryListActionsForKey("End date"));
    }

    [Fact]
    public async Task Get_NonDbsAlertAndUserDoesHaveWritePermission_DoesShowChangeLinks()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsReadWrite));

        var alertType = (await TestData.ReferenceDataCache.GetAlertTypesAsync(activeOnly: true)).RandomOneExcept(at => at.AlertTypeId == AlertType.DbsAlertTypeId);
        var person = await TestData.CreatePersonAsync(b => b
            .WithAlert(a => a.WithStartDate(new(2024, 1, 1)).WithEndDate(new(2024, 10, 10)).WithAlertTypeId(alertType.AlertTypeId)));

        var alert = person.Alerts.Single();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotEmpty(doc.GetSummaryListActionsForKey("End date"));
    }
}

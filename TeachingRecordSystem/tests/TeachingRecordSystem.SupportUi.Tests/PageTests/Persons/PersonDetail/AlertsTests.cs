using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class AlertsTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_PersonDoesNotExists_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_PersonWithNoOpenAlerts_ShowsNoOpenAlertsMessage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b
            .WithAlert(a => a.WithStartDate(new(2024, 1, 1)).WithEndDate(new(2024, 10, 10))));  // Closed alert

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("no-open-alerts"));
    }

    [Fact]
    public async Task Get_PersonWithOpenAlert_ShowsCardWithExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b
            .WithAlert(a => a.WithEndDate(null)));

        var alert = person.Alerts.Single();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("open-alert"),
            card =>
            {
                var title = card.GetElementsByClassName("govuk-summary-card__title").SingleOrDefault();
                Assert.Equal(alert.AlertType.Name, title?.TextContent);

                Assert.Equal(alert.Details, card.GetSummaryListValueForKey("Details"));
                Assert.Equal(alert.ExternalLink, card.GetSummaryListValueElementForKey("Link")?.GetElementsByTagName("a").FirstOrDefault()?.TextContent);
                Assert.Equal(alert.StartDate?.ToString(UiDefaults.DateOnlyDisplayFormat), card.GetSummaryListValueForKey("Start date"));
                Assert.Equal(UiDefaults.EmptyDisplayContent, card.GetSummaryListValueForKey("End date"));
            });
    }

    [Fact]
    public async Task Get_PersonWithNoClosedAlerts_ShowsNoClosedAlertsMessage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b
            .WithAlert(a => a.WithEndDate(null)));  // Open alert

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("no-closed-alerts"));
    }

    [Fact]
    public async Task Get_PersonWithClosedAlert_ShowsTableRowWithExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b
            .WithAlert(a => a.WithStartDate(new(2024, 1, 1)).WithEndDate(new(2024, 10, 10))));

        var alert = person.Alerts.Single();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var closedAlertsTable = doc.GetElementByTestId("closed-alerts");
        Assert.NotNull(closedAlertsTable);

        Assert.Collection(
            closedAlertsTable.GetElementsByTagName("tbody").Single().GetElementsByTagName("tr"),
            row => Assert.Collection(
                row.GetElementsByTagName("td"),
                column => Assert.Equal(alert.AlertType.Name, column.TextContent),
                column => Assert.Equal(alert.StartDate?.ToString(UiDefaults.DateOnlyDisplayFormat), column.TextContent),
                column => Assert.Equal(alert.EndDate?.ToString(UiDefaults.DateOnlyDisplayFormat), column.TextContent),
                column => { }));
    }

    [Fact]
    public async Task Get_UserDoesNotHaveAddAlertPermission_DoesNotShowAddAnAlertButton()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role: null));

        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetElementByTestId("AddAnAlertBtn"));
    }

    [Fact]
    public async Task Get_UserHasAddDbsAlertPermission_DoesShowAddAnAlertButton()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsManagerTra));

        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("AddAnAlertBtn"));
    }

    [Fact]
    public async Task Get_UserHasAddNonDbsAlertPermission_DoesShowAddAnAlertButton()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsManagerTraDbs));

        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("AddAnAlertBtn"));
    }

    [Fact]
    public async Task Get_UserDoesNotHaveReadPermissionToOpenDbsAlert_DoesNotShowAlertCard()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role: null));

        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithAlertTypeId(AlertType.DbsAlertTypeId).WithEndDate(null)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetElementByTestId("open-alert"));
    }

    [Fact]
    public async Task Get_UserDoesHaveReadPermissionToOpenDbsAlert_DoesShowAlertCard()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsManagerTra));

        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithAlertTypeId(AlertType.DbsAlertTypeId).WithEndDate(null)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("open-alert"));
    }

    [Fact]
    public async Task Get_UserHasReadButNotWritePermissionToOpenDbsAlert_DoesNotShowActions()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsManagerTra));

        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithAlertTypeId(AlertType.DbsAlertTypeId).WithEndDate(null)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var card = doc.GetElementByTestId("open-alert");
        Assert.NotNull(card);
        var cardActions = card.QuerySelectorAll(".govuk-summary-card__actions>*");
        Assert.Empty(cardActions);
        var changeActions = card.QuerySelectorAll(".govuk-summary-list__actions>*");
        Assert.Empty(changeActions);
    }

    [Fact]
    public async Task Get_UserHasReadAndWritePermissionsToOpenDbsAlert_DoesShowActions()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsManagerTraDbs));

        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithAlertTypeId(AlertType.DbsAlertTypeId).WithEndDate(null)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var card = doc.GetElementByTestId("open-alert");
        Assert.NotNull(card);
        var cardActions = card.QuerySelectorAll(".govuk-summary-card__actions>*");
        Assert.NotEmpty(cardActions);
        var changeActions = card.QuerySelectorAll(".govuk-summary-list__actions>*");
        Assert.NotEmpty(changeActions);
    }

    [Fact]
    public async Task Get_UserHasReadButNotWritePermissionToOpenNonDbsAlert_DoesNotShowActions()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role: null));

        var alertType = (await TestData.ReferenceDataCache.GetAlertTypesAsync(activeOnly: true)).RandomOneExcept(at => at.AlertTypeId == AlertType.DbsAlertTypeId);
        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId).WithEndDate(null)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var card = doc.GetElementByTestId("open-alert");
        Assert.NotNull(card);
        var cardActions = card.QuerySelectorAll(".govuk-summary-card__actions>*");
        Assert.Empty(cardActions);
        var changeActions = card.QuerySelectorAll(".govuk-summary-list__actions>*");
        Assert.Empty(changeActions);
    }

    [Fact]
    public async Task Get_UserHasReadAndWritePermissionsToOpenNonDbsAlert_DoesShowActions()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsManagerTra));

        var alertType = (await TestData.ReferenceDataCache.GetAlertTypesAsync(activeOnly: true)).RandomOneExcept(at => at.AlertTypeId == AlertType.DbsAlertTypeId);
        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId).WithEndDate(null)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var card = doc.GetElementByTestId("open-alert");
        Assert.NotNull(card);
        var cardActions = card.QuerySelectorAll(".govuk-summary-card__actions>*");
        Assert.NotEmpty(cardActions);
        var changeActions = card.QuerySelectorAll(".govuk-summary-list__actions>*");
        Assert.NotEmpty(changeActions);
    }

    [Fact]
    public async Task Get_UserDoesNotHaveReadPermissionToClosedDbsAlert_DoesNotShowAlertRow()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role: null));

        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithAlertTypeId(AlertType.DbsAlertTypeId).WithStartDate(new DateOnly(2024, 1, 1)).WithEndDate(new DateOnly(2024, 10, 1))));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetElementByTestId("closed-alerts"));
    }

    [Fact]
    public async Task Get_UserDoesHaveReadPermissionToClosedDbsAlert_DoesShowAlertRow()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsManagerTra));

        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithAlertTypeId(AlertType.DbsAlertTypeId).WithStartDate(new DateOnly(2024, 1, 1)).WithEndDate(new DateOnly(2024, 10, 1))));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("closed-alerts"));
    }

    [Fact]
    public async Task Get_UserHasReadButNotWritePermissionToClosedDbsAlert_DoesNotShowActions()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsManagerTra));

        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithAlertTypeId(AlertType.DbsAlertTypeId).WithStartDate(new DateOnly(2024, 1, 1)).WithEndDate(new DateOnly(2024, 10, 1))));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var closedAlerts = doc.GetElementByTestId("closed-alerts");
        Assert.NotNull(closedAlerts);
        var row = closedAlerts.QuerySelectorAll("tbody>tr").First();
        var actionColumn = row.QuerySelectorAll("td").Last();
        Assert.Equal("", actionColumn.TextContent.Trim());
    }

    [Fact]
    public async Task Get_UserHasReadAndWritePermissionsToClosedDbsAlert_DoesShowActions()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsManagerTraDbs));

        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithAlertTypeId(AlertType.DbsAlertTypeId).WithStartDate(new DateOnly(2024, 1, 1)).WithEndDate(new DateOnly(2024, 10, 1))));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var closedAlerts = doc.GetElementByTestId("closed-alerts");
        Assert.NotNull(closedAlerts);
        var row = closedAlerts.QuerySelectorAll("tbody>tr").First();
        var actionColumn = row.QuerySelectorAll("td").Last();
        Assert.Equal("Delete alert", actionColumn.TextContent.Trim());
    }

    [Fact]
    public async Task Get_UserHasReadButNotWritePermissionToClosedNonDbsAlert_DoesNotShowActions()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role: null));

        var alertType = (await TestData.ReferenceDataCache.GetAlertTypesAsync(activeOnly: true)).RandomOneExcept(at => at.AlertTypeId == AlertType.DbsAlertTypeId);
        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId).WithStartDate(new DateOnly(2024, 1, 1)).WithEndDate(new DateOnly(2024, 10, 1))));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var closedAlerts = doc.GetElementByTestId("closed-alerts");
        Assert.NotNull(closedAlerts);
        var row = closedAlerts.QuerySelectorAll("tbody>tr").First();
        var actionColumn = row.QuerySelectorAll("td").Last();
        Assert.Equal("", actionColumn.TextContent.Trim());
    }

    [Fact]
    public async Task Get_UserHasReadAndWritePermissionsToClosedNonDbsAlert_DoesShowActions()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsManagerTra));

        var alertType = (await TestData.ReferenceDataCache.GetAlertTypesAsync(activeOnly: true)).RandomOneExcept(at => at.AlertTypeId == AlertType.DbsAlertTypeId);
        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId).WithStartDate(new DateOnly(2024, 1, 1)).WithEndDate(new DateOnly(2024, 10, 1))));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var closedAlerts = doc.GetElementByTestId("closed-alerts");
        Assert.NotNull(closedAlerts);
        var row = closedAlerts.QuerySelectorAll("tbody>tr").First();
        var actionColumn = row.QuerySelectorAll("td").Last();
        Assert.Equal("Delete alert", actionColumn.TextContent.Trim());
    }

    [Fact]
    public async Task Get_PersonHasOpenDbsAlertButUserCannotRead_ShowsFlagMessage()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role: null));

        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithAlertTypeId(AlertType.DbsAlertTypeId).WithEndDate(null)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("OpenAlertFlag"));
    }

    [Fact]
    public async Task Get_PersonHasClosedDbsAlertAndUserCannotRead_DoesNotShowFlagMessage()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role: null));

        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithAlertTypeId(AlertType.DbsAlertTypeId).WithStartDate(new DateOnly(2024, 1, 1)).WithEndDate(new DateOnly(2024, 10, 1))));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetElementByTestId("OpenAlertFlag"));
    }

    [Fact]
    public async Task Get_PersonHasOpenDbsAlertAndUserCanRead_DoesNotShowFlagMessage()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsManagerTra));

        var person = await TestData.CreatePersonAsync(p => p
            .WithAlert(a => a.WithAlertTypeId(AlertType.DbsAlertTypeId).WithStartDate(new DateOnly(2024, 1, 1)).WithEndDate(new DateOnly(2024, 10, 1))));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetElementByTestId("OpenAlertFlag"));
    }
}

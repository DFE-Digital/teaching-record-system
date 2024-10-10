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
        var person = await TestData.CreatePerson(b => b
            .WithAlert(a => a.WithStartDate(new(2024, 1, 1)).WithEndDate(new(2024, 10, 10))));  // Closed alert

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.NotNull(doc.GetElementByTestId("no-open-alerts"));
    }

    [Fact]
    public async Task Get_PersonWithOpenAlert_ShowsCardWithExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b
            .WithAlert(a => a.WithEndDate(null)));

        var alert = person.Alerts.Single();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("open-alert"),
            card =>
            {
                var title = card.GetElementsByClassName("govuk-summary-card__title").SingleOrDefault();
                Assert.Equal(alert.AlertType.Name, title?.TextContent);

                Assert.Equal(alert.Details, card.GetSummaryListValueForKey("Details"));
                Assert.Equal(alert.ExternalLink, card.GetSummaryListValueElementForKey("Link")?.GetElementsByTagName("a").FirstOrDefault()?.TextContent);
                Assert.Equal(alert.StartDate?.ToString("d MMMM yyyy"), card.GetSummaryListValueForKey("Start date"));
                Assert.Equal("-", card.GetSummaryListValueForKey("End date"));
            });
    }

    [Fact]
    public async Task Get_PersonWithNoClosedAlerts_ShowsNoClosedAlertsMessage()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b
            .WithAlert(a => a.WithEndDate(null)));  // Open alert

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.NotNull(doc.GetElementByTestId("no-closed-alerts"));
    }

    [Fact]
    public async Task Get_PersonWithClosedAlert_ShowsTableRowWithExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b
            .WithAlert(a => a.WithStartDate(new(2024, 1, 1)).WithEndDate(new(2024, 10, 10))));

        var alert = person.Alerts.Single();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        var closedAlertsTable = doc.GetElementByTestId("closed-alerts");
        Assert.NotNull(closedAlertsTable);

        Assert.Collection(
            closedAlertsTable.GetElementsByTagName("tbody").Single().GetElementsByTagName("tr"),
            row => Assert.Collection(
                row.GetElementsByTagName("td"),
                column => Assert.Equal(alert.AlertType.Name, column.TextContent),
                column => Assert.Equal(alert.StartDate?.ToString("d MMMM yyyy"), column.TextContent),
                column => Assert.Equal(alert.EndDate?.ToString("d MMMM yyyy"), column.TextContent),
                column => { }));
    }
}

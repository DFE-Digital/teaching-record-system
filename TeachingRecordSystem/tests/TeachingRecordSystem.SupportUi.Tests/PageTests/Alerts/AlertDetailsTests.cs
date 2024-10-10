namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts;

public class AlertDetailsTests(HostFixture hostFixture) : TestBase(hostFixture)
{
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
        var person = await TestData.CreatePerson(b => b
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
        var person = await TestData.CreatePerson(b => b
            .WithAlert(a => a.WithStartDate(new(2024, 1, 1)).WithEndDate(new(2024, 10, 10))));

        var alert = person.Alerts.Single();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        var h1 = doc.GetElementsByTagName("h1").Single();
        Assert.Equal(alert.AlertType.Name, h1.TextContent);

        Assert.Equal(alert.Details, doc.GetSummaryListValueForKey("Details"));
        Assert.Equal(alert.StartDate?.ToString("d MMMM yyyy"), doc.GetSummaryListValueForKey("Start date"));
        Assert.Equal(alert.EndDate?.ToString("d MMMM yyyy"), doc.GetSummaryListValueForKey("End date"));
    }
}

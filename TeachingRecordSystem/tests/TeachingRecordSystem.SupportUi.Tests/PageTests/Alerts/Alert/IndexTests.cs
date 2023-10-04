using TeachingRecordSystem.SupportUi.Pages.Common;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.Alert;

public class IndexTests : TestBase
{
    public IndexTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var nonExistentAlertId = Guid.NewGuid().ToString();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{nonExistentAlertId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Theory]
    [InlineData("2021-01-01", "2022-03-05", true, true, AlertStatus.Closed)]
    [InlineData("2021-01-01", null, false, true, AlertStatus.Active)]
    [InlineData("2021-01-01", null, false, false, AlertStatus.Inactive)]
    [InlineData("2021-01-01", "2022-03-05", true, false, AlertStatus.Inactive)]
    public async Task Get_ValidRequest_RendersExpectedContent(string startDateString, string? endDateString, bool isSpent, bool isActive, AlertStatus expectedAlertStatus)
    {
        // Arrange
        var sanctionCode = "G1";
        var sanctionCodeName = (await TestData.ReferenceDataCache.GetSanctionCodeByValue(sanctionCode)).dfeta_name;
        var startDate = DateOnly.Parse(startDateString);
        DateOnly? endDate = endDateString is not null ? DateOnly.Parse(endDateString) : null;
        var person = await TestData.CreatePerson(x => x.WithSanction(sanctionCode, startDate: startDate, endDate: endDate, spent: isSpent, isActive: isActive));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{person.Sanctions.Single().SanctionId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();

        Assert.Equal(sanctionCodeName, doc.GetElementByTestId("title")!.TextContent);

        var alertHeader = doc.GetElementByTestId("alert-header");
        Assert.NotNull(alertHeader);
        Assert.Equal(startDate.ToString("dd/MM/yyyy"), alertHeader.GetElementByTestId("start-date")!.TextContent);
        Assert.Equal(endDate is not null ? endDate.Value.ToString("dd/MM/yyyy") : "-", alertHeader.GetElementByTestId("end-date")!.TextContent);
        Assert.Equal(expectedAlertStatus.ToString(), alertHeader.GetElementByTestId("status")!.TextContent);
        Assert.NotNull(doc.GetElementByTestId("alert-details"));
        if (isActive)
        {
            Assert.NotNull(doc.GetElementByTestId("deactivate-button"));
            Assert.Null(doc.GetElementByTestId("reactivate-button"));
        }
        else
        {
            Assert.Null(doc.GetElementByTestId("deactivate-button"));
            Assert.NotNull(doc.GetElementByTestId("reactivate-button"));
        }
    }
}

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.CloseAlert;

public class ConfirmTests : TestBase
{
    public ConfirmTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var nonExistentAlertId = Guid.NewGuid().ToString();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{nonExistentAlertId}/close/confirm");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_RendersExpectedContent()
    {
        // Arrange
        var sanctionCode = "G1";
        var sanctionCodeName = (await TestData.ReferenceDataCache.GetSanctionCodeByValue(sanctionCode)).dfeta_name;
        var startDate = new DateOnly(2021, 01, 01);
        var endDate = new DateOnly(2022, 03, 05);
        var person = await TestData.CreatePerson(x => x.WithSanction(sanctionCode, startDate: startDate));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{person.Sanctions.Single().SanctionId}/close/confirm?endDate={endDate:yyyy-MM-dd}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        Assert.Equal(sanctionCodeName, doc.GetElementByTestId("alert-type")!.TextContent);
        Assert.Equal(endDate.ToString("dd/MM/yyyy"), doc.GetElementByTestId("end-date")!.TextContent);
    }

    [Fact]
    public async Task Post_ValidRequest_ClosesAlert()
    {
        // Arrange
        var sanctionCode = "G1";
        var startDate = new DateOnly(2021, 01, 01);
        var endDate = new DateOnly(2023, 08, 02);
        var person = await TestData.CreatePerson(x => x.WithSanction(sanctionCode, startDate: startDate));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
           $"/alerts/{person.Sanctions.Single().SanctionId}/close/confirm?endDate={endDate:yyyy-MM-dd}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Alert closed");
    }
}

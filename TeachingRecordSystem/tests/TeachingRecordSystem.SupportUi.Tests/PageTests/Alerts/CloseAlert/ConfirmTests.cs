using TeachingRecordSystem.SupportUi.Pages.Alerts.CloseAlert;

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
        var endDate = new DateOnly(2020, 01, 01);
        var alertId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstance(alertId, new CloseAlertState() { EndDate = endDate });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/close/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_StateHasNoEndDate_RedirectsToIndex()
    {
        // Arrange
        var sanctionCode = "G1";
        var sanctionCodeName = (await TestData.ReferenceDataCache.GetSanctionCodeByValue(sanctionCode)).dfeta_name;
        var startDate = new DateOnly(2021, 01, 01);
        var endDate = new DateOnly(2020, 01, 01);
        var person = await TestData.CreatePerson(x => x.WithSanction(sanctionCode, startDate: startDate));
        var alertId = person.Sanctions.Single().SanctionId;

        var journeyInstance = await CreateJourneyInstance(alertId, new CloseAlertState() { EndDate = null });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/close/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/alerts/{alertId}/close?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequest_RendersExpectedContent()
    {
        // Arrange
        var sanctionCode = "G1";
        var sanctionCodeName = (await TestData.ReferenceDataCache.GetSanctionCodeByValue(sanctionCode)).dfeta_name;
        var startDate = new DateOnly(2021, 01, 01);
        var endDate = new DateOnly(2020, 01, 01);
        var person = await TestData.CreatePerson(x => x.WithSanction(sanctionCode, startDate: startDate));
        var alertId = person.Sanctions.Single().SanctionId;

        var journeyInstance = await CreateJourneyInstance(alertId, new CloseAlertState() { EndDate = endDate });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/close/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        Assert.Equal(sanctionCodeName, doc.GetElementByTestId("alert-type")!.TextContent);
        Assert.Equal(endDate.ToString("dd/MM/yyyy"), doc.GetElementByTestId("end-date")!.TextContent);
    }

    [Fact]
    public async Task Post_ValidRequest_ClosesAlertAndCompletesJourney()
    {
        // Arrange
        var sanctionCode = "G1";
        var startDate = new DateOnly(2021, 01, 01);
        var endDate = new DateOnly(2023, 08, 02);
        var person = await TestData.CreatePerson(x => x.WithSanction(sanctionCode, startDate: startDate));
        var alertId = person.Sanctions.Single().SanctionId;

        var journeyInstance = await CreateJourneyInstance(alertId, new CloseAlertState() { EndDate = endDate });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/close/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
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

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    private async Task<JourneyInstance<CloseAlertState>> CreateJourneyInstance(Guid alertId, CloseAlertState state) =>
        await CreateJourneyInstance(
            JourneyNames.CloseAlert,
            state,
            new KeyValuePair<string, object>("alertId", alertId));
}

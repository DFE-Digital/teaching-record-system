using FormFlow;
using TeachingRecordSystem.SupportUi.Pages.Alerts.CloseAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.CloseAlert;

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
        var alertId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/close?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithAlertIdForEndedAlert_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePerson(x => x.WithSanction("G1", endDate: new DateOnly(2023, 09, 21)));
        var alertId = person.Sanctions.Single().SanctionId;

        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/close?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithAlertIdForActiveAlert_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePerson(x => x.WithSanction("G1"));
        var alertId = person.Sanctions.Single().SanctionId;

        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/close?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
    }

    [Fact]
    public async Task Get_ValidRequestWithEndDateInJourneyState_PopulatesEndDateFromJourneyState()
    {
        // Arrange
        var startDate = new DateOnly(2021, 01, 01);
        var endDate = new DateOnly(2020, 01, 01);
        var person = await TestData.CreatePerson(x => x.WithSanction("G1", startDate: startDate));
        var alertId = person.Sanctions.Single().SanctionId;

        var journeyInstance = await CreateJourneyInstance(alertId, new CloseAlertState() { EndDate = endDate });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/close?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocument();
        Assert.Equal($"{endDate:%d}", doc.GetElementById("EndDate.Day")?.GetAttribute("value"));
        Assert.Equal($"{endDate:%M}", doc.GetElementById("EndDate.Month")?.GetAttribute("value"));
        Assert.Equal($"{endDate:yyyy}", doc.GetElementById("EndDate.Year")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Post_EmptyEndDate_ReturnsError()
    {
        // Arrange        
        var person = await TestData.CreatePerson(x => x.WithSanction("G1"));
        var alertId = person.Sanctions.Single().SanctionId;

        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/close?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "EndDate", "Add an end date");
    }

    [Fact]
    public async Task Post_EndDateBeforeStartDate_ReturnsError()
    {
        // Arrange
        var startDate = new DateOnly(2021, 01, 01);
        var endDate = new DateOnly(2020, 01, 01);
        var person = await TestData.CreatePerson(x => x.WithSanction("G1", startDate: startDate));
        var alertId = person.Sanctions.Single().SanctionId;

        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/close?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "EndDate.Day", endDate.Day.ToString() },
                { "EndDate.Month", endDate.Month.ToString() },
                { "EndDate.Year", endDate.Year.ToString() },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "EndDate", "End date must be after the start date");
    }

    [Fact]
    public async Task Post_ValidEndDate_RedirectsToConfirmPage()
    {
        // Arrange
        var startDate = new DateOnly(2021, 01, 01);
        var endDate = new DateOnly(2022, 08, 03);
        var person = await TestData.CreatePerson(x => x.WithSanction("G1", startDate: startDate));
        var alertId = person.Sanctions.Single().SanctionId;

        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/close?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "EndDate.Day", endDate.Day.ToString() },
                { "EndDate.Month", endDate.Month.ToString() },
                { "EndDate.Year", endDate.Year.ToString() },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/alerts/{alertId}/close/confirm?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    private async Task<JourneyInstance<CloseAlertState>> CreateJourneyInstance(Guid alertId, CloseAlertState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.CloseAlert,
            state ?? new CloseAlertState(),
            new KeyValuePair<string, object>("alertId", alertId));
}

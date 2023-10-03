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
        var nonExistentActivityId = Guid.NewGuid().ToString();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{nonExistentActivityId}/close");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{person.Sanctions.Single().SanctionId}/close");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{person.Sanctions.Single().SanctionId}/close");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
    }

    [Fact]
    public async Task Get_ValidRequestWithEndDateInQueryParam_PopulatesEndDateFromQueryParam()
    {
        // Arrange
        var startDate = new DateOnly(2021, 01, 01);
        var endDate = new DateOnly(2020, 01, 01);
        var person = await TestData.CreatePerson(x => x.WithSanction("G1", startDate: startDate));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/alerts/{person.Sanctions.Single().SanctionId}/close?endDate={endDate:yyyy-MM-dd}");

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

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/alerts/{person.Sanctions.Single().SanctionId}/close")
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

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/alerts/{person.Sanctions.Single().SanctionId}/close")
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

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/alerts/{person.Sanctions.Single().SanctionId}/close")
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
        Assert.Equal($"/alerts/{person.Sanctions.Single().SanctionId}/close/confirm?endDate={endDate:yyyy-MM-dd}", response.Headers.Location?.OriginalString);
    }
}

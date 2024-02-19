namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class AlertsTests : TestBase
{
    public AlertsTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange        
        var nonExistentPersonId = Guid.NewGuid().ToString();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{nonExistentPersonId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithNoAlerts_DisplaysNoCurrentAlerts()
    {
        // Arrange        
        var person = await TestData.CreatePerson();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var noCurrentAlerts = doc.GetElementByTestId("no-current-alerts");
        Assert.NotNull(noCurrentAlerts);
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithOnlyPreviousAlerts_DisplaysExpectedContent()
    {
        // Arrange
        var sanctionCode = "G1";
        var sanctionCodeName = (await TestData.ReferenceDataCache.GetSanctionCodeByValue(sanctionCode)).dfeta_name;
        var sanctionStartDate = new DateOnly(2023, 05, 01);
        var sanctionEndDate = new DateOnly(2023, 08, 20);
        var person = await TestData.CreatePerson(x => x.WithSanction(sanctionCode, startDate: sanctionStartDate, endDate: sanctionEndDate));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        var noCurrentAlerts = doc.GetElementByTestId("no-current-alerts");
        Assert.NotNull(noCurrentAlerts);

        var previousAlerts = doc.GetElementByTestId("previous-alerts");
        Assert.NotNull(previousAlerts);

        var previousAlert = previousAlerts.GetElementByTestId($"previous-alert-{person.Sanctions.First().SanctionId}");
        Assert.NotNull(previousAlert);
        Assert.Equal(sanctionCodeName, previousAlert.GetElementByTestId($"previous-alert-description-{person.Sanctions.First().SanctionId}")!.TextContent);
        Assert.Equal(sanctionStartDate.ToString("dd/MM/yyyy"), previousAlert.GetElementByTestId($"previous-alert-start-date-{person.Sanctions.First().SanctionId}")!.TextContent);
        Assert.Equal(sanctionEndDate.ToString("dd/MM/yyyy"), previousAlert.GetElementByTestId($"previous-alert-end-date-{person.Sanctions.First().SanctionId}")!.TextContent);
        Assert.Equal("Closed", previousAlert.GetElementByTestId($"previous-alert-status-{person.Sanctions.First().SanctionId}")!.TextContent);
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithOnlyCurrentAlerts_DisplaysExpectedContent()
    {
        // Arrange
        var sanctionCode = "G1";
        var sanctionCodeName = (await TestData.ReferenceDataCache.GetSanctionCodeByValue(sanctionCode)).dfeta_name;
        var sanctionStartDate = new DateOnly(2023, 05, 01);
        var person = await TestData.CreatePerson(x => x.WithSanction(sanctionCode, startDate: sanctionStartDate));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        var currentAlert = doc.GetElementByTestId($"current-alert-{person.Sanctions.First().SanctionId}");
        Assert.NotNull(currentAlert);
        Assert.Equal(sanctionStartDate.ToString("dd/MM/yyyy"), currentAlert.GetElementByTestId($"current-alert-start-date-{person.Sanctions.First().SanctionId}")!.TextContent);
        var details = currentAlert.GetElementByTestId($"current-alert-details-{person.Sanctions.First().SanctionId}");
        Assert.NotNull(details);

        var previousAlerts = doc.GetElementByTestId("previous-alerts");
        Assert.Null(previousAlerts);
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithCurrentAndPreviousAlerts_DisplaysExpectedContent()
    {
        // Arrange
        var sanction1Code = "G1";
        var sanction1CodeName = (await TestData.ReferenceDataCache.GetSanctionCodeByValue(sanction1Code)).dfeta_name;
        var sanction1StartDate = new DateOnly(2023, 05, 01);

        var sanction2Code = "A1";
        var sanction2CodeName = (await TestData.ReferenceDataCache.GetSanctionCodeByValue(sanction2Code)).dfeta_name;
        var sanction2StartDate = new DateOnly(2019, 06, 24);
        var sanction2EndDate = new DateOnly(2020, 07, 30);

        var person = await TestData.CreatePerson(x => x.WithSanction(sanction1Code, startDate: sanction1StartDate).WithSanction(sanction2Code, startDate: sanction2StartDate, endDate: sanction2EndDate));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/alerts");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        var currentAlert = doc.GetElementByTestId($"current-alert-{person.Sanctions.First().SanctionId}");
        Assert.NotNull(currentAlert);
        Assert.Equal(sanction1StartDate.ToString("dd/MM/yyyy"), currentAlert.GetElementByTestId($"current-alert-start-date-{person.Sanctions.First().SanctionId}")!.TextContent);
        var details = currentAlert.GetElementByTestId($"current-alert-details-{person.Sanctions.First().SanctionId}");
        Assert.NotNull(details);

        var previousAlerts = doc.GetElementByTestId("previous-alerts");
        Assert.NotNull(previousAlerts);

        var previousAlert = previousAlerts.GetElementByTestId($"previous-alert-{person.Sanctions.Last().SanctionId}");
        Assert.NotNull(previousAlert);
        Assert.Equal(sanction2CodeName, previousAlert.GetElementByTestId($"previous-alert-description-{person.Sanctions.Last().SanctionId}")!.TextContent);
        Assert.Equal(sanction2StartDate.ToString("dd/MM/yyyy"), previousAlert.GetElementByTestId($"previous-alert-start-date-{person.Sanctions.Last().SanctionId}")!.TextContent);
        Assert.Equal(sanction2EndDate.ToString("dd/MM/yyyy"), previousAlert.GetElementByTestId($"previous-alert-end-date-{person.Sanctions.Last().SanctionId}")!.TextContent);
        Assert.Equal("Closed", previousAlert.GetElementByTestId($"previous-alert-status-{person.Sanctions.Last().SanctionId}")!.TextContent);
    }
}

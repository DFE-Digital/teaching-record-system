namespace TeachingRecordSystem.Api.Tests.V3.V20240920;

public class GetPersonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_ValidRequestWithAlerts_ReturnsExpectedAlertsContent()
    {
        // Arrange
        var sanctionCode = "A13";
        var startDate = new DateOnly(2022, 4, 1);
        var endDate = new DateOnly(2023, 1, 20);
        var alertType = await ReferenceDataCache.GetAlertTypeByDqtSanctionCode(sanctionCode);
        var alertCategory = await ReferenceDataCache.GetAlertCategoryById(alertType.AlertCategoryId);

        var person = await TestData.CreatePerson(x => x
            .WithTrn()
            .WithSanction(sanctionCode, startDate, endDate));

        var sanction = person.Sanctions.Single();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/person?include=Alerts");

        var httpClient = GetHttpClientWithIdentityAccessToken(person.Trn!);

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponse(response);
        var responseAlerts = jsonResponse.RootElement.GetProperty("alerts");

        AssertEx.JsonObjectEquals(
            new[]
            {
                new
                {
                    alertId = sanction.SanctionId,
                    alertType = new
                    {
                        alertTypeId = alertType.AlertTypeId,
                        name = alertType.Name,
                        alertCategory = new
                        {
                            alertCategoryId = alertCategory.AlertCategoryId,
                            name = alertCategory.Name
                        }
                    },
                    startDate = startDate,
                    endDate = endDate
                }
            },
            responseAlerts);
    }
}

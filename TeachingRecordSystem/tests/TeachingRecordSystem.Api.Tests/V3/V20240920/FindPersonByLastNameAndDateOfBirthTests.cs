namespace TeachingRecordSystem.Api.Tests.V3.V20240920;

[Collection(nameof(DisableParallelization))]
public class FindPersonByLastNameAndDateOfBirthTests : TestBase
{
    public FindPersonByLastNameAndDateOfBirthTests(HostFixture hostFixture) : base(hostFixture)
    {
        XrmFakedContext.DeleteAllEntities<Contact>();
        SetCurrentApiClient([ApiRoles.GetPerson]);
    }

    [Fact]
    public async Task Get_ValidRequestWithMatchOnPersonWithAlerts_ReturnsExpectedAlertsContent()
    {
        // Arrange
        var lastName = "Smith";
        var dateOfBirth = new DateOnly(1990, 1, 1);

        var sanctionCode = "A13";
        var startDate = new DateOnly(2022, 4, 1);
        var endDate = new DateOnly(2023, 1, 20);
        var alertType = await ReferenceDataCache.GetAlertTypeByDqtSanctionCode(sanctionCode);
        var alertCategory = await ReferenceDataCache.GetAlertCategoryById(alertType.AlertCategoryId);

        var person = await TestData.CreatePerson(b => b
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithSanction(sanctionCode, startDate, endDate));

        var sanction = person.Sanctions.Single();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/v3/persons/find")
        {
            Content = JsonContent.Create(new
            {
                persons = new[]
                {
                    new
                    {
                        trn = person.Trn,
                        dateOfBirth = person.DateOfBirth
                    }
                }
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponse(response);
        var responseAlerts = jsonResponse.RootElement.GetProperty("results").EnumerateArray().Single().GetProperty("alerts");

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
                    endDate = endDate,
                }
            },
            responseAlerts);
    }
}

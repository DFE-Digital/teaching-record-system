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

        var alertTypes = await TestData.ReferenceDataCache.GetAlertTypes();
        var alertType = alertTypes.Where(at => !at.InternalOnly).RandomOne();

        var person = await TestData.CreatePerson(p => p
            .WithTrn()
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId).WithEndDate(null)));

        var alert = person.Alerts.Single();

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
                    alertId = alert.AlertId,
                    alertType = new
                    {
                        alertTypeId = alert.AlertType.AlertTypeId,
                        name = alert.AlertType.Name,
                        alertCategory = new
                        {
                            alertCategoryId = alert.AlertType.AlertCategory.AlertCategoryId,
                            name = alert.AlertType.AlertCategory.Name
                        }
                    },
                    details = alert.Details,
                    startDate = alert.StartDate,
                    endDate = alert.EndDate,
                }
            },
            responseAlerts);
    }
}

using TeachingRecordSystem.Api.V3.V20240920.Requests;

namespace TeachingRecordSystem.Api.Tests.V3.V20240920;

public class GetPersonByTrnTests : TestBase
{
    public GetPersonByTrnTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentApiClient(roles: [ApiRoles.GetPerson]);
    }

    [Theory, RoleNamesData(except: [ApiRoles.GetPerson, ApiRoles.AppropriateBody])]
    public async Task Get_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);

        var person = await TestData.CreatePerson(x => x.WithTrn());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?dateOfBirth={person.DateOfBirth:yyyy-MM-dd}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(GetPersonRequestIncludes.InitialTeacherTraining)]
    [InlineData(GetPersonRequestIncludes.NpqQualifications)]
    [InlineData(GetPersonRequestIncludes.MandatoryQualifications)]
    [InlineData(GetPersonRequestIncludes.PendingDetailChanges)]
    [InlineData(GetPersonRequestIncludes.HigherEducationQualifications)]
    [InlineData(GetPersonRequestIncludes.PreviousNames)]
    [InlineData(GetPersonRequestIncludes._AllowIdSignInWithProhibitions)]
    public async Task Get_AsAppropriateBodyWithNotPermittedInclude_ReturnsForbidden(GetPersonRequestIncludes include)
    {
        // Arrange
        SetCurrentApiClient(roles: [ApiRoles.AppropriateBody]);

        var person = await TestData.CreatePerson(x => x.WithTrn());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?dateOfBirth={person.DateOfBirth:yyyy-MM-dd}&include={Uri.EscapeDataString(include.ToString())}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(GetPersonRequestIncludes.Induction)]
    [InlineData(GetPersonRequestIncludes.Alerts)]
    public async Task Get_AsAppropriateBodyWithPermittedInclude_ReturnsOk(GetPersonRequestIncludes include)
    {
        // Arrange
        SetCurrentApiClient(roles: [ApiRoles.AppropriateBody]);

        var person = await TestData.CreatePerson(x => x.WithTrn());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?dateOfBirth={person.DateOfBirth:yyyy-MM-dd}&include={Uri.EscapeDataString(include.ToString())}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_AsAppropriateBodyWithoutDateOfBirth_ReturnsForbidden()
    {
        // Arrange
        SetCurrentApiClient(roles: [ApiRoles.AppropriateBody]);

        var person = await TestData.CreatePerson(x => x.WithTrn());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_ReturnsExpectedAlertsContent()
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?include=Alerts");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

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
                    endDate = endDate,
                }
            },
            responseAlerts);
    }
}

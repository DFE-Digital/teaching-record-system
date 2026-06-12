using System.Text.Json;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20240912;

public class GetPersonByTrnTests : TestBase
{
    public GetPersonByTrnTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.GetPerson]);
    }

    [Fact]
    public async Task Get_UnauthenticatedRequest_ReturnsUnauthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/persons/1234567");

        // Act
        var response = await GetHttpClient(Version).SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }

    [Theory, RoleNamesData(except: [ApiRoles.GetPerson])]
    public async Task Get_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);

        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_TrnNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/persons/1234567");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_DateOfBirthDoesNotMatchTeachingRecord_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var dateOfBirth = person.DateOfBirth.AddDays(1);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?dateOfBirth={dateOfBirth:yyyy-MM-dd}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_DateOfBirthMatchesTeachingRecord_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?dateOfBirth={person.DateOfBirth:yyyy-MM-dd}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_DateOfBirthNotProvided_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_ReturnsExpectedResponse()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithNationalInsuranceNumber()
            .WithEmailAddress(Faker.Internet.Email()));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                firstName = person.FirstName,
                middleName = person.MiddleName,
                lastName = person.LastName,
                trn = person.Trn,
                dateOfBirth = person.DateOfBirth.ToString("yyyy-MM-dd"),
                nationalInsuranceNumber = person.NationalInsuranceNumber,
                qts = (object?)null,
                eyts = (object?)null,
                emailAddress = person.EmailAddress
            },
            expectedStatusCode: StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Get_ValidRequestWithInduction_ReturnsExpectedInductionContent()
    {
        // Arrange
        var startDate = new DateOnly(1996, 2, 3);
        var completedDate = new DateOnly(1996, 6, 7);

        var person = await TestData.CreatePersonAsync(p => p
            .WithInductionStatus(i => i
                .WithStatus(InductionStatus.Passed)
                .WithStartDate(startDate)
                .WithCompletedDate(completedDate)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?include=Induction");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseInduction = jsonResponse.RootElement.GetProperty("induction");

        AssertEx.JsonObjectEquals(
            new
            {
                startDate = startDate.ToString("yyyy-MM-dd"),
                endDate = completedDate.ToString("yyyy-MM-dd"),
                status = "Pass",
                statusDescription = "Pass",
                certificateUrl = "/v3/certificates/induction",
                periods = Array.Empty<object>()
            },
            responseInduction);
    }

    [Fact]
    public async Task Get_ValidRequestWithInductionAndPersonHasNoInductionStatus_ReturnsNullInductionContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?include=Induction");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseInduction = jsonResponse.RootElement.GetProperty("induction");
        Assert.Equal(JsonValueKind.Null, responseInduction.ValueKind);
    }

    [Fact]
    public async Task Get_ValidRequestWithMandatoryQualifications_ReturnsExpectedMandatoryQualificationsContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            // MQ with no EndDate
            .WithMandatoryQualification(b => b.WithStatus(MandatoryQualificationStatus.InProgress))
            // MQ with no Specialism
            .WithMandatoryQualification(b => b.WithSpecialism(null))
            // MQ with EndDate and Specialism
            .WithMandatoryQualification(b => b
                .WithStatus(MandatoryQualificationStatus.Passed, endDate: new(2022, 9, 1))
                .WithSpecialism(MandatoryQualificationSpecialism.Auditory)));

        var validMq = person.MandatoryQualifications.Last();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?include=MandatoryQualifications");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseMandatoryQualifications = jsonResponse.RootElement.GetProperty("mandatoryQualifications");

        AssertEx.JsonObjectEquals(
            new[]
            {
                new
                {
                    awarded = validMq.EndDate?.ToString("yyyy-MM-dd"),
                    specialism = validMq.Specialism?.GetTitle()
                }
            },
            responseMandatoryQualifications);
    }

    [Fact]
    public async Task Get_ValidRequestWithSanctions_ReturnsExpectedSanctionsContent()
    {
        // Arrange
        var alertTypes = await TestData.ReferenceDataCache.GetAlertTypesAsync();
        var alertType = alertTypes.Where(at => Api.V3.Constants.LegacyExposableSanctionCodes.Contains(at.DqtSanctionCode)).SingleRandom();

        var person = await TestData.CreatePersonAsync(b => b
            .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId)));

        var alert = person.Alerts.Single();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?include=Sanctions");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseSanctions = jsonResponse.RootElement.GetProperty("sanctions");

        AssertEx.JsonObjectEquals(
            new[]
            {
                new
                {
                    code = alertType.DqtSanctionCode,
                    startDate = alert.StartDate
                }
            },
            responseSanctions);
    }

    [Fact]
    public async Task Get_ValidRequestWithAlerts_ReturnsExpectedAlertsContent()
    {
        // Arrange
        var alertTypes = await TestData.ReferenceDataCache.GetAlertTypesAsync();
        var alertType = alertTypes.Where(at => Api.V3.Constants.LegacyProhibitionSanctionCodes.Contains(at.DqtSanctionCode)).SingleRandom();

        var person = await TestData.CreatePersonAsync(b => b
            .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId)));

        var alert = person.Alerts.Single();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?include=Alerts");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseAlerts = jsonResponse.RootElement.GetProperty("alerts");

        AssertEx.JsonObjectEquals(
            new[]
            {
                new
                {
                    alertType = "Prohibition",
                    dqtSanctionCode = alertType.DqtSanctionCode,
                    startDate = alert.StartDate,
                    endDate = alert.EndDate
                }
            },
            responseAlerts);
    }

    [Fact]
    public async Task Get_ValidRequestWithPreviousNames_ReturnsExpectedPreviousNamesContent()
    {
        // Arrange
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();

        var person = await TestData.CreatePersonAsync(p => p
            .WithPreviousNames((firstName, middleName, lastName, new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc))));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?include=PreviousNames");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responsePreviousNames = jsonResponse.RootElement.GetProperty("previousNames");

        AssertEx.JsonObjectEquals(
            new[]
            {
                new
                {
                    firstName,
                    middleName,
                    lastName
                }
            },
            responsePreviousNames);
    }
}

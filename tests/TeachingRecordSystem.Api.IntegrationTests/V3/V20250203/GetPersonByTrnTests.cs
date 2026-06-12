using System.Text.Json;
using TeachingRecordSystem.Api.V3.V20250203.Requests;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20250203;

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

    [Theory, RoleNamesData(except: [ApiRoles.GetPerson, ApiRoles.AppropriateBody])]
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
    public async Task Get_WithNationalInsuranceNumberMatchingRecord_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?nationalInsuranceNumber={person.NationalInsuranceNumber}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithNationalInsuranceNumberNotMatchingRecord_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());
        var requestNino = TestData.GenerateChangedNationalInsuranceNumber(person.NationalInsuranceNumber!);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?nationalInsuranceNumber={requestNino}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_BothNationalInsuranceNumberAndDateOfBirthSpecified_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons/{person.Trn}?dateOfBirth={person.DateOfBirth:yyyy-MM-dd}&nationalInsuranceNumber={person.NationalInsuranceNumber}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
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
                emailAddress = person.EmailAddress,
                qtlsStatus = "None"
            },
            expectedStatusCode: StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Get_ValidRequestWithQts_ReturnsExpectedQtsContentWithoutCertificateUrl()
    {
        // Arrange
        var qtsDate = new DateOnly(2021, 1, 1);
        var person = await TestData.CreatePersonAsync(p => p.WithQts(qtsDate));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseQts = jsonResponse.RootElement.GetProperty("qts");

        Assert.Equal(qtsDate.ToString("yyyy-MM-dd"), responseQts.GetProperty("awarded").GetString());
        Assert.False(responseQts.TryGetProperty("certificateUrl", out _));
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
                status = InductionStatus.Passed.ToString(),
                startDate = startDate.ToString("yyyy-MM-dd"),
                completedDate = completedDate.ToString("yyyy-MM-dd")
            },
            responseInduction);
    }

    [Fact]
    public async Task Get_ValidRequestWithInductionAndPersonHasNoInductionStatus_ReturnsNoneInductionStatus()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?include=Induction");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseInduction = jsonResponse.RootElement.GetProperty("induction");

        AssertEx.JsonObjectEquals(
            new
            {
                status = InductionStatus.None.ToString(),
                startDate = (DateOnly?)null,
                completedDate = (DateOnly?)null
            },
            responseInduction);
    }

    [Fact]
    public async Task Get_WithQtlsDate_ReturnsActiveQtlsStatus()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQtls(new DateOnly(2020, 1, 1)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        Assert.Equal("Active", jsonResponse.RootElement.GetProperty("qtlsStatus").GetString());
    }

    [Fact]
    public async Task Get_WithExpiredQtls_ReturnsExpiredQtlsStatus()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQtlsStatus(QtlsStatus.Expired));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        Assert.Equal("Expired", jsonResponse.RootElement.GetProperty("qtlsStatus").GetString());
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
    public async Task Get_ValidRequestWithAlerts_ReturnsExpectedAlertsContent()
    {
        // Arrange
        var alertTypes = await TestData.ReferenceDataCache.GetAlertTypesAsync();
        var alertType = alertTypes.Where(at => !at.InternalOnly).SingleRandom();

        var person = await TestData.CreatePersonAsync(x => x
            .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId).WithEndDate(null)));

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
                    alertId = alert.AlertId,
                    alertType = new
                    {
                        alertTypeId = alert.AlertType!.AlertTypeId,
                        name = alert.AlertType.Name,
                        alertCategory = new
                        {
                            alertCategoryId = alert.AlertType.AlertCategory!.AlertCategoryId,
                            name = alert.AlertType.AlertCategory.Name
                        }
                    },
                    details = alert.Details,
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

    [Theory]
    [InlineData(GetPersonRequestIncludes.Induction)]
    [InlineData(GetPersonRequestIncludes.Alerts)]
    [InlineData(GetPersonRequestIncludes.InitialTeacherTraining)]
    public async Task Get_AsAppropriateBodyWithPermittedInclude_ReturnsOk(GetPersonRequestIncludes include)
    {
        // Arrange
        SetCurrentApiClient([ApiRoles.AppropriateBody]);

        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?dateOfBirth={person.DateOfBirth:yyyy-MM-dd}&include={Uri.EscapeDataString(include.ToString())}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(GetPersonRequestIncludes.NpqQualifications)]
    [InlineData(GetPersonRequestIncludes.MandatoryQualifications)]
    [InlineData(GetPersonRequestIncludes.PendingDetailChanges)]
    [InlineData(GetPersonRequestIncludes.PreviousNames)]
    [InlineData(GetPersonRequestIncludes._AllowIdSignInWithProhibitions)]
    public async Task Get_AsAppropriateBodyWithNotPermittedInclude_ReturnsForbidden(GetPersonRequestIncludes include)
    {
        // Arrange
        SetCurrentApiClient([ApiRoles.AppropriateBody]);

        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?dateOfBirth={person.DateOfBirth:yyyy-MM-dd}&include={Uri.EscapeDataString(include.ToString())}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_AsAppropriateBodyWithoutDateOfBirth_ReturnsForbidden()
    {
        // Arrange
        SetCurrentApiClient([ApiRoles.AppropriateBody]);

        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_AsAppropriateBodySpecifiesNationalInsuranceNumber_ReturnsForbidden()
    {
        // Arrange
        SetCurrentApiClient([ApiRoles.AppropriateBody]);

        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?nationalInsuranceNumber={person.NationalInsuranceNumber}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }
}

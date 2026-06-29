namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20250425;

public class GetPersonTests : TestBase
{
    public GetPersonTests(HostFixture hostFixture) : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_UnauthenticatedRequest_ReturnsUnauthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person");

        // Act
        var response = await GetHttpClient(Version).SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_PersonForTrnDoesNotExist_ReturnsForbidden()
    {
        // Arrange
        var httpClient = GetHttpClientWithAuthorizeAccessToken("1234567", Version);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithTrnClaim_ReturnsExpectedResponse()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithNationalInsuranceNumber()
            .WithEmailAddress(Faker.Internet.Email()));

        var httpClient = GetHttpClientWithAuthorizeAccessToken(person.Trn!, Version);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person");

        // Act
        var response = await httpClient.SendAsync(request);

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
    public async Task Get_WithTrnRequestIdClaimAndResolvedRequest_ReturnsPersonDetails()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequestId = Guid.NewGuid().ToString();

        await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId, t => t
            .WithRequestId(trnRequestId)
            .WithResolvedPersonId(person.PersonId));

        var httpClient = GetHttpClientWithAuthorizeAccessTokenForTrnRequest(applicationUser.UserId, trnRequestId, Version);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        Assert.Equal(person.Trn, jsonResponse.RootElement.GetProperty("trn").GetString());
    }

    [Fact]
    public async Task Get_WithTrnRequestIdClaimButUnresolvedRequest_ReturnsForbidden()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequestId = Guid.NewGuid().ToString();

        await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId, t => t
            .WithRequestId(trnRequestId));

        var httpClient = GetHttpClientWithAuthorizeAccessTokenForTrnRequest(applicationUser.UserId, trnRequestId, Version);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithQts_ReturnsExpectedQtsContentWithoutCertificateUrl()
    {
        // Arrange
        var qtsDate = new DateOnly(2021, 1, 1);
        var person = await TestData.CreatePersonAsync(p => p.WithQts(qtsDate));

        var httpClient = GetHttpClientWithAuthorizeAccessToken(person.Trn!, Version);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseQts = jsonResponse.RootElement.GetProperty("qts");

        Assert.Equal(qtsDate.ToString("yyyy-MM-dd"), responseQts.GetProperty("awarded").GetString());
        Assert.Equal(1, responseQts.GetProperty("awardedOrApprovedCount").GetInt32());
        Assert.False(responseQts.TryGetProperty("certificateUrl", out _));
    }

    [Fact]
    public async Task Get_PersonWithQtlsAndQtsViaAnotherRoute_ReturnsExpectedAwardedOrApprovedCount()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithQtls(TimeProvider.Today));

        var httpClient = GetHttpClientWithAuthorizeAccessToken(person.Trn!, Version);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        var responseJson = await AssertEx.JsonResponseAsync(response);
        var awardedOrApprovedCount = responseJson.RootElement.GetProperty("qts").GetProperty("awardedOrApprovedCount").GetInt32();
        Assert.Equal(2, awardedOrApprovedCount);
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

        var httpClient = GetHttpClientWithAuthorizeAccessToken(person.Trn!, Version);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person?include=Induction");

        // Act
        var response = await httpClient.SendAsync(request);

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

        var httpClient = GetHttpClientWithAuthorizeAccessToken(person.Trn!, Version);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person?include=Induction");

        // Act
        var response = await httpClient.SendAsync(request);

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

        var httpClient = GetHttpClientWithAuthorizeAccessToken(person.Trn!, Version);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        Assert.Equal("Active", jsonResponse.RootElement.GetProperty("qtlsStatus").GetString());
    }

    [Fact]
    public async Task Get_WithExpiredQtls_ReturnsExpiredQtlsStatus()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQtlsStatus(QtlsStatus.Expired));

        var httpClient = GetHttpClientWithAuthorizeAccessToken(person.Trn!, Version);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person");

        // Act
        var response = await httpClient.SendAsync(request);

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

        var httpClient = GetHttpClientWithAuthorizeAccessToken(person.Trn!, Version);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person?include=MandatoryQualifications");

        // Act
        var response = await httpClient.SendAsync(request);

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

        var httpClient = GetHttpClientWithAuthorizeAccessToken(person.Trn!, Version);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person?include=Alerts");

        // Act
        var response = await httpClient.SendAsync(request);

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

        var httpClient = GetHttpClientWithAuthorizeAccessToken(person.Trn!, Version);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person?include=PreviousNames");

        // Act
        var response = await httpClient.SendAsync(request);

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

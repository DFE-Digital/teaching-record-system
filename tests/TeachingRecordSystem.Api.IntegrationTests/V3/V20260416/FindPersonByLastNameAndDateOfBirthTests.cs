namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20260416;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class FindPersonByLastNameAndDateOfBirthTests : TestBase
{
    public FindPersonByLastNameAndDateOfBirthTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.GetPerson]);
    }

    [Fact]
    public async Task Get_UnauthenticatedRequest_ReturnsUnauthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/v3/persons?findBy=LastNameAndDateOfBirth&lastName=Smith&dateOfBirth=1990-01-01");

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

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/v3/persons?findBy=LastNameAndDateOfBirth&lastName=Smith&dateOfBirth=1990-01-01");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Theory]
    [InlineData("", "Invalid matching policy.")]
    [InlineData("BadFindBy", "The value 'BadFindBy' is not valid for FindBy.")]
    public async Task Get_InvalidFindBy_ReturnsError(string findBy, string expectedErrorMessage)
    {
        // Arrange
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons?findBy={findBy}&lastName=Smith&dateOfBirth=1990-01-01");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, "findBy", expectedErrorMessage);
    }

    [Theory]
    [InlineData("", "1990-01-01", "lastName", "A value is required when findBy is 'LastNameAndDateOfBirth'.")]
    [InlineData("Smith", "", "dateOfBirth", "A value is required when findBy is 'LastNameAndDateOfBirth'.")]
    public async Task Get_MissingPropertiesForFindBy_ReturnsError(
        string lastName,
        string dateOfBirth,
        string expectedErrorPropertyName,
        string expectedErrorMessage)
    {
        // Arrange
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons?findBy=LastNameAndDateOfBirth&lastName={lastName}&dateOfBirth={dateOfBirth}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, expectedErrorPropertyName, expectedErrorMessage);
    }

    [Fact]
    public async Task Get_NoMatch_ReturnsEmptyResults()
    {
        // Arrange
        var findBy = "LastNameAndDateOfBirth";
        var lastName = TestData.GenerateLastName();
        var dateOfBirth = new DateOnly(1990, 1, 1);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons?findBy={findBy}&lastName={lastName}&dateOfBirth={dateOfBirth:yyyy-MM-dd}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEqualsAsync(
            response,
            new
            {
                total = 0,
                query = new { findBy, lastName, dateOfBirth },
                results = Array.Empty<object>()
            });
    }

    [Fact]
    public async Task Get_ValidRequestWithMatch_ReturnsExpectedResult()
    {
        // Arrange
        var findBy = "LastNameAndDateOfBirth";
        var lastName = "Smith";
        var dateOfBirth = new DateOnly(1990, 1, 1);

        var person = await TestData.CreatePersonAsync(p => p
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons?findBy={findBy}&lastName={lastName}&dateOfBirth={dateOfBirth:yyyy-MM-dd}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEqualsAsync(
            response,
            new
            {
                total = 1,
                query = new { findBy, lastName, dateOfBirth },
                results = new[]
                {
                    new
                    {
                        trn = person.Trn,
                        dateOfBirth = person.DateOfBirth,
                        firstName = person.FirstName,
                        middleName = person.MiddleName,
                        lastName = person.LastName,
                        previousNames = Array.Empty<object>(),
                        qts = (object?)null,
                        eyts = (object?)null,
                        alerts = Array.Empty<object>(),
                        induction = new
                        {
                            status = InductionStatus.None.ToString(),
                            startDate = (DateOnly?)null,
                            completedDate = (DateOnly?)null,
                            exemptionReasons = Array.Empty<object>()
                        },
                        qtlsStatus = "None"
                    }
                }
            });
    }

    [Fact]
    public async Task Get_PersonHasInduction_ReturnsExpectedInductionContent()
    {
        // Arrange
        var lastName = "Smith";
        var dateOfBirth = new DateOnly(1990, 1, 1);
        var startDate = new DateOnly(1996, 2, 3);
        var completedDate = new DateOnly(1996, 6, 7);

        var person = await TestData.CreatePersonAsync(p => p
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithInductionStatus(i => i
                .WithStatus(InductionStatus.Passed)
                .WithStartDate(startDate)
                .WithCompletedDate(completedDate)));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons?findBy=LastNameAndDateOfBirth&lastName={lastName}&dateOfBirth={dateOfBirth:yyyy-MM-dd}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseInduction = jsonResponse.RootElement.GetProperty("results").EnumerateArray().Single().GetProperty("induction");

        AssertEx.JsonObjectEquals(
            new
            {
                status = InductionStatus.Passed.ToString(),
                startDate = startDate.ToString("yyyy-MM-dd"),
                completedDate = completedDate.ToString("yyyy-MM-dd"),
                exemptionReasons = Array.Empty<object>()
            },
            responseInduction);
    }

    [Fact]
    public async Task Get_WithQtlsDate_ReturnsActiveQtlsStatus()
    {
        // Arrange
        var lastName = "Smith";
        var dateOfBirth = new DateOnly(1990, 1, 1);

        var person = await TestData.CreatePersonAsync(p => p
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithQtls(new DateOnly(2020, 1, 1)));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons?findBy=LastNameAndDateOfBirth&lastName={lastName}&dateOfBirth={dateOfBirth:yyyy-MM-dd}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var qtlsStatus = jsonResponse.RootElement.GetProperty("results").EnumerateArray().Single().GetProperty("qtlsStatus").GetString();
        Assert.Equal("Active", qtlsStatus);
    }

    [Fact]
    public async Task Get_WithExpiredQtls_ReturnsExpiredQtlsStatus()
    {
        // Arrange
        var lastName = "Smith";
        var dateOfBirth = new DateOnly(1990, 1, 1);

        var person = await TestData.CreatePersonAsync(p => p
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithQtlsStatus(QtlsStatus.Expired));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons?findBy=LastNameAndDateOfBirth&lastName={lastName}&dateOfBirth={dateOfBirth:yyyy-MM-dd}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var qtlsStatus = jsonResponse.RootElement.GetProperty("results").EnumerateArray().Single().GetProperty("qtlsStatus").GetString();
        Assert.Equal("Expired", qtlsStatus);
    }

    [Fact]
    public async Task Get_PersonWithQts_ReturnsExpectedQtsContent()
    {
        // Arrange
        var lastName = "Smith";
        var dateOfBirth = new DateOnly(1990, 1, 1);
        var qtsDate = new DateOnly(2021, 1, 1);

        var person = await TestData.CreatePersonAsync(p => p
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithQts(qtsDate));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons?findBy=LastNameAndDateOfBirth&lastName={lastName}&dateOfBirth={dateOfBirth:yyyy-MM-dd}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var qts = jsonResponse.RootElement.GetProperty("results").EnumerateArray().Single().GetProperty("qts");

        Assert.Equal(qtsDate.ToString("yyyy-MM-dd"), qts.GetProperty("holdsFrom").GetString());
        Assert.True(qts.GetProperty("routes").GetArrayLength() >= 1);
        Assert.False(qts.TryGetProperty("awarded", out _));
        Assert.False(qts.TryGetProperty("awardedOrApprovedCount", out _));
    }
}

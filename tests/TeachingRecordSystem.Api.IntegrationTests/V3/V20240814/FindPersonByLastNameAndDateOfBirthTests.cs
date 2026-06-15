namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20240814;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class FindPersonByLastNameAndDateOfBirthTests : TestBase
{
    public FindPersonByLastNameAndDateOfBirthTests(HostFixture hostFixture)
        : base(hostFixture)
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
                        sanctions = Array.Empty<object>(),
                        previousNames = Array.Empty<object>(),
                        inductionStatus = (object?)null,
                        qts = (object?)null,
                        eyts = (object?)null
                    }
                }
            });
    }

    [Fact]
    public async Task Get_ValidRequestWithMatchOnPersonWithInduction_ReturnsExpectedInductionStatus()
    {
        // Arrange
        var lastName = "Smith";
        var dateOfBirth = new DateOnly(1990, 1, 1);

        var person = await TestData.CreatePersonAsync(p => p
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithInductionStatus(i => i
                .WithStatus(InductionStatus.Passed)
                .WithStartDate(new DateOnly(1996, 2, 3))
                .WithCompletedDate(new DateOnly(1996, 6, 7))));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons?findBy=LastNameAndDateOfBirth&lastName={lastName}&dateOfBirth={dateOfBirth:yyyy-MM-dd}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseInductionStatus = jsonResponse.RootElement.GetProperty("results").EnumerateArray().Single().GetProperty("inductionStatus");

        AssertEx.JsonObjectEquals(
            new
            {
                status = "Pass",
                statusDescription = "Pass"
            },
            responseInductionStatus);
    }

    [Fact]
    public async Task Get_NonExposableSanctionCode_IsNotReturned()
    {
        // Arrange
        var findBy = "LastNameAndDateOfBirth";
        var lastName = "Smith";
        var dateOfBirth = new DateOnly(1990, 1, 1);

        var alertTypes = await TestData.ReferenceDataCache.GetAlertTypesAsync();
        var alertType = alertTypes.Where(at => !Api.V3.Constants.LegacyExposableSanctionCodes.Contains(at.DqtSanctionCode)).SingleRandom();

        var person = await TestData.CreatePersonAsync(b => b
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId).WithEndDate(null)));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons?findBy={findBy}&lastName={lastName}&dateOfBirth={dateOfBirth:yyyy-MM-dd}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseSanctions = jsonResponse.RootElement.GetProperty("results").EnumerateArray().Single().GetProperty("sanctions");
        Assert.Equal(0, responseSanctions.GetArrayLength());
    }
}

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20240814;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class FindPersonsByTrnAndDateOfBirthTests : TestBase
{
    public FindPersonsByTrnAndDateOfBirthTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.GetPerson]);
    }

    [Fact]
    public async Task Post_UnauthenticatedRequest_ReturnsUnauthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/persons/find")
        {
            Content = CreateJsonContent(new { persons = new[] { new { trn = "1234567", dateOfBirth = new DateOnly(1990, 1, 1) } } })
        };

        // Act
        var response = await GetHttpClient(Version).SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }

    [Theory, RoleNamesData(except: [ApiRoles.GetPerson])]
    public async Task Post_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/persons/find")
        {
            Content = CreateJsonContent(new { persons = new[] { new { trn = "1234567", dateOfBirth = new DateOnly(1990, 1, 1) } } })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_NoMatch_ReturnsEmptyResults()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/persons/find")
        {
            Content = CreateJsonContent(new { persons = new[] { new { trn = "1234567", dateOfBirth = new DateOnly(1990, 1, 1) } } })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEqualsAsync(
            response,
            new
            {
                total = 0,
                results = Array.Empty<object>()
            });
    }

    [Fact]
    public async Task Post_ValidRequestWithMatch_ReturnsExpectedResult()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/persons/find")
        {
            Content = CreateJsonContent(new { persons = new[] { new { trn = person.Trn, dateOfBirth = person.DateOfBirth } } })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEqualsAsync(
            response,
            new
            {
                total = 1,
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
    public async Task Post_PersonWithInduction_ReturnsExpectedInductionStatus()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithInductionStatus(i => i
                .WithStatus(InductionStatus.Passed)
                .WithStartDate(new DateOnly(1996, 2, 3))
                .WithCompletedDate(new DateOnly(1996, 6, 7))));

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/persons/find")
        {
            Content = CreateJsonContent(new { persons = new[] { new { trn = person.Trn, dateOfBirth = person.DateOfBirth } } })
        };

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
}

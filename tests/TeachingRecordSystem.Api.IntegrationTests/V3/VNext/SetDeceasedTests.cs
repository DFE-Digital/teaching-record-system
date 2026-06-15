using System.Net;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.VNext;

[Collection(nameof(DisableParallelization))]
public class SetDeceasedTests : TestBase
{
    public SetDeceasedTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.UpdatePerson]);
    }

    [Theory, RoleNamesData(except: ApiRoles.UpdatePerson)]
    public async Task Put_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);
        var person = await TestData.CreatePersonAsync();
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/deceased/{person.Trn}")
        {
            Content = CreateJsonContent(new { dateOfDeath = new DateOnly(1990, 01, 01) })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Put_DateOfDeathInFuture_ReturnsError()
    {
        // Arrange
        var futureDate = Clock.Today.AddDays(1);
        var person = await TestData.CreatePersonAsync();
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/deceased/{person.Trn}")
        {
            Content = CreateJsonContent(new { dateOfDeath = futureDate })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(
            response,
            propertyName: "DateOfDeath",
            expectedError: "Date cannot be in the future.");
    }

    [Fact]
    public async Task Put_WithoutDateOfDeath_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/deceased/{person.Trn}")
        {
            Content = CreateJsonContent(new { dateOfDeath = default(DateOnly?) })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_TrnNotFound_ReturnsBadRequest()
    {
        // Arrange
        var nonExistentTrn = "1234567";
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/deceased/{nonExistentTrn}")
        {
            Content = CreateJsonContent(new { dateOfDeath = new DateOnly(1990, 01, 01) })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_PersonAlreadyHasDateOfDeath_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var request1 = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/deceased/{person.Trn}")
        {
            Content = CreateJsonContent(new { dateOfDeath = new DateOnly(1990, 01, 01) })
        };
        var request2 = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/deceased/{person.Trn}")
        {
            Content = CreateJsonContent(new { dateOfDeath = new DateOnly(1980, 01, 01) })
        };

        // Act
        var response1 = await GetHttpClientWithApiKey().SendAsync(request1);
        var response2 = await GetHttpClientWithApiKey().SendAsync(request2);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response1.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
    }

    [Fact]
    public async Task Put_ValidDateOfDeath_ReturnsNoContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/deceased/{person.Trn}")
        {
            Content = CreateJsonContent(new { dateOfDeath = new DateOnly(1990, 01, 01) })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}

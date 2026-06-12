namespace TeachingRecordSystem.Api.IntegrationTests.V3.VNext;

// Note: the CreateAlert endpoint (POST /v3/alerts) is currently an unimplemented stub
// (the controller action throws NotImplementedException), so only the authentication/authorization
// layer — which runs before the action — is asserted here. Behavioural tests should be added once the
// operation is implemented.
public class CreateAlertTests : TestBase
{
    public CreateAlertTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.UpdateRole]);
    }

    [Fact]
    public async Task Post_UnauthenticatedRequest_ReturnsUnauthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/alerts")
        {
            Content = CreateJsonContent(new { })
        };

        // Act
        var response = await GetHttpClient(Version).SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }

    [Theory, RoleNamesData(except: ApiRoles.UpdateRole)]
    public async Task Post_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);

        var request = new HttpRequestMessage(HttpMethod.Post, "/v3/alerts")
        {
            Content = CreateJsonContent(new { })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }
}

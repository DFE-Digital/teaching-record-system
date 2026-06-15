namespace TeachingRecordSystem.Api.IntegrationTests.V3.VNext;

// Note: the GetAlert endpoint (GET /v3/alerts/{alertId}) is currently an unimplemented stub
// (the controller action throws NotImplementedException), so only the authentication/authorization
// layer — which runs before the action — is asserted here. Behavioural tests should be added once the
// operation is implemented.
public class GetAlertTests : TestBase
{
    public GetAlertTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.UpdateRole]);
    }

    [Fact]
    public async Task Get_UnauthenticatedRequest_ReturnsUnauthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/alerts/{Guid.NewGuid()}");

        // Act
        var response = await GetHttpClient(Version).SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }

    [Theory, RoleNamesData(except: ApiRoles.UpdateRole)]
    public async Task Get_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/alerts/{Guid.NewGuid()}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }
}

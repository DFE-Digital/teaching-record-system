namespace TeachingRecordSystem.Api.IntegrationTests.V3.VNext;

// Note: the UpdateAlert endpoint (PATCH /v3/alerts/{alertId}) is currently an unimplemented stub
// (the controller action throws NotImplementedException), so only the authentication/authorization
// layer — which runs before the action — is asserted here. Behavioural tests should be added once the
// operation is implemented.
public class UpdateAlertTests : TestBase
{
    public UpdateAlertTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.UpdateRole]);
    }

    [Fact]
    public async Task Patch_UnauthenticatedRequest_ReturnsUnauthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Patch, $"/v3/alerts/{Guid.NewGuid()}")
        {
            Content = CreateJsonContent(new { })
        };

        // Act
        var response = await GetHttpClient(Version).SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }

    [Theory, RoleNamesData(except: ApiRoles.UpdateRole)]
    public async Task Patch_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/v3/alerts/{Guid.NewGuid()}")
        {
            Content = CreateJsonContent(new { })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }
}

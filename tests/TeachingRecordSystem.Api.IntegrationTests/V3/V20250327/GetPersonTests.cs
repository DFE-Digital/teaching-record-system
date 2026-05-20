namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20250327;

[Collection(nameof(DisableParallelization)), ClearDbBeforeTest]
public class GetPersonTests : TestBase
{
    public GetPersonTests(HostFixture hostFixture) : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_WithTrnClaim_ReturnsPersonDetails()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var httpClient = GetHttpClientWithAuthorizeAccessToken(person.Trn!, Version);
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

        await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, t => t
            .WithRequestId(trnRequestId));

        var httpClient = GetHttpClientWithAuthorizeAccessTokenForTrnRequest(applicationUser.UserId, trnRequestId, Version);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/person");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithTrnRequestIdClaimAndResolvedRequest_ReturnsPersonDetails()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequestId = Guid.NewGuid().ToString();

        await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, t => t
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
}

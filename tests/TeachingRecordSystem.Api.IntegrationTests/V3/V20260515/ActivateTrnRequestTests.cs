namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20260515;

[Collection(nameof(DisableParallelization)), ClearDbBeforeTest]
public class ActivateTrnRequestTests(HostFixture hostFixture) : V20260416.TestBase(hostFixture)
{
    [Fact]
    public async Task Put_TrnRequestIsNotDormant_ReturnsOk()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequestId = Guid.NewGuid().ToString();

        var person = await TestData.CreatePersonAsync(p => p.WithTrnRequest(applicationUser.UserId, trnRequestId));

        var httpClient = GetHttpClientWithAuthorizeAccessTokenForTrnRequest(applicationUser.UserId, trnRequestId);

        var request = new HttpRequestMessage(HttpMethod.Put, "/v3/trn-request/activate");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response, expectedStatusCode: StatusCodes.Status200OK);
        Assert.Equal("Completed", jsonResponse.RootElement.GetProperty("status").GetString());
        Assert.NotNull(jsonResponse.RootElement.GetProperty("trn").GetString());
    }

    [Fact]
    public async Task Put_ValidRequestForDormantRequest_ReturnsOk()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var trnRequest = await TestData.CreateDormantTrnRequestAsync(applicationUser.UserId);

        var httpClient = GetHttpClientWithAuthorizeAccessTokenForTrnRequest(applicationUser.UserId, trnRequest.RequestId);

        var request = new HttpRequestMessage(HttpMethod.Put, "/v3/trn-request/activate");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response, expectedStatusCode: StatusCodes.Status200OK);
        Assert.Equal("Completed", jsonResponse.RootElement.GetProperty("status").GetString());
        Assert.NotNull(jsonResponse.RootElement.GetProperty("trn").GetString());
    }
}

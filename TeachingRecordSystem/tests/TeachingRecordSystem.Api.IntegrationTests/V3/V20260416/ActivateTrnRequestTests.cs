using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20260416;

[Collection(nameof(DisableParallelization)), ClearDbBeforeTest]
public class ActivateTrnRequestTests : TestBase
{
    public ActivateTrnRequestTests(HostFixture hostFixture) : base(hostFixture)
    {
        GetAnIdentityApiClientMock
            .Setup(mock => mock.CreateTrnTokenAsync(It.IsAny<CreateTrnTokenRequest>()))
            .ReturnsAsync((CreateTrnTokenRequest req) => new CreateTrnTokenResponse()
            {
                Email = req.Email,
                ExpiresUtc = Clock.UtcNow.AddDays(1),
                Trn = req.Trn,
                TrnToken = Guid.NewGuid().ToString()
            });
    }

    [Fact]
    public async Task Get_TrnRequestIsNotDormant_ReturnsNoContent()
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
        var jsonResponse = await AssertEx.JsonResponseAsync(response, expectedStatusCode: StatusCodes.Status204NoContent);
        Assert.Equal("Completed", jsonResponse.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Get_ValidRequestForDormantRequest_ReturnsOk()
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
    }
}

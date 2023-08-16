namespace TeachingRecordSystem.Api.Tests.V2;

public class SwaggerTests : ApiTestBase
{
    public SwaggerTests(ApiFixture apiFixture) : base(apiFixture)
    {
    }

    [Fact]
    public async Task Get_SwaggerEndpoint_ReturnsOk()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "swagger/v2.json");
        var httpClient = ApiFixture.CreateClient();

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(200, (int)response.StatusCode);
    }
}

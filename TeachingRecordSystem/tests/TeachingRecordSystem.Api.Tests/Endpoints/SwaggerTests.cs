namespace TeachingRecordSystem.Api.Tests.Endpoints;

[TestClass]
public class SwaggerTests : ApiTestBase
{
    public SwaggerTests(ApiFixture apiFixture) : base(apiFixture)
    {
    }

    [Test]
    [InlineData("v1")]
    [InlineData("v2")]
    [InlineData("v3")]
    public async Task Get_SwaggerEndpoint_ReturnsOk(string version)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"swagger/{version}.json");
        var httpClient = ApiFixture.CreateClient();

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(200, (int)response.StatusCode);
    }
}

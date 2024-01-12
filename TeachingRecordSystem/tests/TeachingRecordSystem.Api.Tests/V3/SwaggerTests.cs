namespace TeachingRecordSystem.Api.Tests.V3;

public class SwaggerTests : TestBase
{
    public SwaggerTests(HostFixture hostFixture) : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_SwaggerEndpoint_ReturnsOk()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "swagger/v3.json");
        var httpClient = HostFixture.CreateClient();

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(200, (int)response.StatusCode);
    }
}

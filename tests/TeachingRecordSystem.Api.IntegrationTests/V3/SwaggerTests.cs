namespace TeachingRecordSystem.Api.IntegrationTests.V3;

public class SwaggerTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    public static IEnumerable<object[]> MinorVersions => VersionRegistry.AllV3MinorVersions.Select(v => new object[] { v });

    [Theory]
    [MemberData(nameof(MinorVersions))]
    public async Task Get_SwaggerEndpoint_ReturnsOk(string minorVersion)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"swagger/v3_{minorVersion}.json");
        var httpClient = HostFixture.CreateClient();

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(200, (int)response.StatusCode);
    }
}

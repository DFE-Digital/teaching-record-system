using System.Text.Json;

namespace TeachingRecordSystem.Api.IntegrationTests.V1;

public class SwaggerTests : TestBase
{
    public SwaggerTests(HostFixture hostFixture) : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_SwaggerEndpoint_ReturnsOk()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "swagger/v1.json");
        var httpClient = HostFixture.CreateClient();

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(200, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_SwaggerEndpoint_MarksParametersWithRequiredAttributeAsRequired()
    {
        // Arrange
        var httpClient = HostFixture.CreateClient();

        // Act
        var response = await httpClient.GetAsync("swagger/v1.json");

        // Assert
        Assert.Equal(200, (int)response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var parameter = document.RootElement.GetProperty("paths").GetProperty("/v1/teachers/{trn}").GetProperty("get").GetProperty("parameters")
            .EnumerateArray().Single(p => p.GetProperty("name").GetString() == "birthdate");
        Assert.True(parameter.GetProperty("required").GetBoolean());
    }
}

using System.Text.Json;

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

    [Fact]
    public async Task Get_SwaggerEndpoint_IncludesWebhookMessageSchemasFromThisAndEarlierVersions()
    {
        // Arrange
        var httpClient = HostFixture.CreateClient();

        // Act
        var response = await httpClient.GetAsync($"swagger/v3_{VersionRegistry.V3MinorVersions.V20260612}.json");

        // Assert
        var schemaNames = await GetSchemaNamesAsync(response);
        // Introduced at 20260612
        Assert.Contains("PersonDeactivatedNotification", schemaNames);
        // Introduced at earlier versions but still delivered to endpoints on 20260612
        Assert.Contains("TrnRequestCompletedNotification", schemaNames);
        Assert.Contains("OneLoginUserUpdatedNotification", schemaNames);
        Assert.Contains("AlertCreatedNotification", schemaNames);
        Assert.Contains("AlertUpdatedNotification", schemaNames);
        Assert.Contains("AlertDeletedNotification", schemaNames);
    }

    [Fact]
    public async Task Get_SwaggerEndpoint_ExcludesWebhookMessageSchemasFromLaterVersions()
    {
        // Arrange
        var httpClient = HostFixture.CreateClient();

        // Act
        var response = await httpClient.GetAsync($"swagger/v3_{VersionRegistry.V3MinorVersions.V20250804}.json");

        // Assert
        var schemaNames = await GetSchemaNamesAsync(response);
        Assert.Contains("AlertCreatedNotification", schemaNames);
        // Both were introduced after 20250804
        Assert.DoesNotContain("TrnRequestCompletedNotification", schemaNames);
        Assert.DoesNotContain("PersonDeactivatedNotification", schemaNames);
    }

    [Theory]
    [InlineData(VersionRegistry.V3MinorVersions.V20240920, "initialTeacherTraining", "GetPersonResponseInitialTeacherTraining")]
    [InlineData(VersionRegistry.V3MinorVersions.V20250627, "routesToProfessionalStatuses", "GetPersonResponseRouteToProfessionalStatus")]
    public async Task Get_SwaggerEndpoint_DescribesUnionPropertyWithItsFirstCaseType(string minorVersion, string propertyName, string expectedItemSchemaName)
    {
        // Arrange
        var httpClient = HostFixture.CreateClient();

        // Act
        var response = await httpClient.GetAsync($"swagger/v3_{minorVersion}.json");

        // Assert
        Assert.Equal(200, (int)response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var property = document.RootElement
            .GetProperty("components")
            .GetProperty("schemas")
            .GetProperty("GetPersonResponse")
            .GetProperty("properties")
            .GetProperty(propertyName);
        Assert.Equal("array", property.GetProperty("type").GetString());
        Assert.Equal($"#/components/schemas/{expectedItemSchemaName}", property.GetProperty("items").GetProperty("$ref").GetString());
    }

    private static async Task<IReadOnlyCollection<string>> GetSchemaNamesAsync(HttpResponseMessage response)
    {
        Assert.Equal(200, (int)response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        return document.RootElement
            .GetProperty("components")
            .GetProperty("schemas")
            .EnumerateObject()
            .Select(p => p.Name)
            .ToArray();
    }
}

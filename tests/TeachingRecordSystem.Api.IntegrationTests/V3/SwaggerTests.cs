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

    [Fact]
    public async Task Get_SwaggerEndpoint_DocumentsOptionPropertiesAsOptionalValueType()
    {
        // Arrange
        var httpClient = HostFixture.CreateClient();

        // Act
        var response = await httpClient.GetAsync($"swagger/v3_{VersionRegistry.V3MinorVersions.VNext}.json");

        // Assert
        using var document = await GetDocumentAsync(response);
        var schema = document.RootElement.GetProperty("components").GetProperty("schemas").GetProperty("UpdateAlertRequestBody");
        Assert.False(schema.TryGetProperty("required", out _));
        var properties = schema.GetProperty("properties");
        // Option<DateOnly>
        Assert.Equal("string", properties.GetProperty("startDate").GetProperty("type").GetString());
        Assert.Equal("date", properties.GetProperty("startDate").GetProperty("format").GetString());
        // Option<DateOnly?>
        Assert.Equal(["null", "string"], GetTypes(properties.GetProperty("endDate")));
        Assert.Equal("date", properties.GetProperty("endDate").GetProperty("format").GetString());
        // Option<string?>
        Assert.Equal(["null", "string"], GetTypes(properties.GetProperty("details")));
    }

    [Fact]
    public async Task Get_SwaggerEndpoint_DocumentsNullableObjectOptionPropertyAsNullableReference()
    {
        // Arrange
        var httpClient = HostFixture.CreateClient();

        // Act
        var response = await httpClient.GetAsync($"swagger/v3_{VersionRegistry.V3MinorVersions.V20240101}.json");

        // Assert
        using var document = await GetDocumentAsync(response);
        var schema = document.RootElement.GetProperty("components").GetProperty("schemas").GetProperty("GetTeacherResponse");
        Assert.DoesNotContain("induction", schema.GetProperty("required").EnumerateArray().Select(e => e.GetString()));
        // Option<GetTeacherResponseInduction?>
        var oneOf = schema.GetProperty("properties").GetProperty("induction").GetProperty("oneOf").EnumerateArray().ToArray();
        Assert.Collection(
            oneOf,
            s => Assert.Equal("null", s.GetProperty("type").GetString()),
            s => Assert.Equal("#/components/schemas/GetTeacherResponseInduction", s.GetProperty("$ref").GetString()));
    }

    [Fact]
    public async Task Get_SwaggerEndpoint_DocumentsOneOfPropertyAsAnyOfItsTypes()
    {
        // Arrange
        var httpClient = HostFixture.CreateClient();

        // Act
        var response = await httpClient.GetAsync($"swagger/v3_{VersionRegistry.V3MinorVersions.V20250627}.json");

        // Assert
        using var document = await GetDocumentAsync(response);
        var schemas = document.RootElement.GetProperty("components").GetProperty("schemas");
        Assert.DoesNotContain(schemas.EnumerateObject(), s => s.Name.StartsWith("OneOf"));
        // Option<OneOf<IReadOnlyCollection<GetPersonResponseRouteToProfessionalStatus>, IReadOnlyCollection<GetPersonResponseRouteToProfessionalStatusForAppropriateBody>>>
        var anyOf = schemas.GetProperty("GetPersonResponse").GetProperty("properties").GetProperty("routesToProfessionalStatuses")
            .GetProperty("anyOf").EnumerateArray().ToArray();
        Assert.Collection(
            anyOf,
            s => Assert.Equal("#/components/schemas/GetPersonResponseRouteToProfessionalStatus", s.GetProperty("items").GetProperty("$ref").GetString()),
            s => Assert.Equal("#/components/schemas/GetPersonResponseRouteToProfessionalStatusForAppropriateBody", s.GetProperty("items").GetProperty("$ref").GetString()));
    }

    [Fact]
    public async Task Get_SwaggerEndpoint_DocumentsEnumsAsStrings()
    {
        // Arrange
        var httpClient = HostFixture.CreateClient();

        // Act
        var response = await httpClient.GetAsync($"swagger/v3_{VersionRegistry.V3MinorVersions.V20250627}.json");

        // Assert
        using var document = await GetDocumentAsync(response);
        var schema = document.RootElement.GetProperty("components").GetProperty("schemas").GetProperty("InductionStatus");
        Assert.Contains("Passed", schema.GetProperty("enum").EnumerateArray().Select(e => e.GetString()));
    }

    [Fact]
    public async Task Get_SwaggerEndpoint_AddsVersionHeaderParameterReferencingVersionSchema()
    {
        // Arrange
        var minorVersion = VersionRegistry.V3MinorVersions.V20250627;
        var httpClient = HostFixture.CreateClient();

        // Act
        var response = await httpClient.GetAsync($"swagger/v3_{minorVersion}.json");

        // Assert
        using var document = await GetDocumentAsync(response);
        var parameter = document.RootElement.GetProperty("paths").GetProperty("/v3/persons/{trn}").GetProperty("get").GetProperty("parameters")
            .EnumerateArray().Single(p => p.GetProperty("name").GetString() == VersionRegistry.MinorVersionHeaderName);
        Assert.True(parameter.GetProperty("required").GetBoolean());
        Assert.Equal("#/components/schemas/VersionHeader", parameter.GetProperty("schema").GetProperty("$ref").GetString());
        var schema = document.RootElement.GetProperty("components").GetProperty("schemas").GetProperty("VersionHeader");
        Assert.Equal(minorVersion, schema.GetProperty("const").GetString());
    }

    [Fact]
    public async Task Get_SwaggerEndpoint_DocumentsProblemDetailsResponsesAsProblemJson()
    {
        // Arrange
        var httpClient = HostFixture.CreateClient();

        // Act
        var response = await httpClient.GetAsync($"swagger/v3_{VersionRegistry.V3MinorVersions.V20250627}.json");

        // Assert
        using var document = await GetDocumentAsync(response);
        var content = document.RootElement.GetProperty("paths").GetProperty("/v3/persons/{trn}").GetProperty("get").GetProperty("responses")
            .GetProperty("404").GetProperty("content");
        var mediaType = Assert.Single(content.EnumerateObject());
        Assert.Equal("application/problem+json", mediaType.Name);
        Assert.Equal("#/components/schemas/ProblemDetails", mediaType.Value.GetProperty("schema").GetProperty("$ref").GetString());
    }

    [Fact]
    public async Task Get_SwaggerEndpoint_ReferencesComponentsFromWebhookMessageSchemas()
    {
        // Arrange
        var httpClient = HostFixture.CreateClient();

        // Act
        var response = await httpClient.GetAsync($"swagger/v3_{VersionRegistry.V3MinorVersions.V20250804}.json");

        // Assert
        using var document = await GetDocumentAsync(response);
        var schema = document.RootElement.GetProperty("components").GetProperty("schemas").GetProperty("AlertCreatedNotification");
        Assert.Equal("#/components/schemas/Alert", schema.GetProperty("properties").GetProperty("alert").GetProperty("$ref").GetString());
    }

    private static string?[] GetTypes(JsonElement schema) =>
        schema.GetProperty("type").EnumerateArray().Select(e => e.GetString()).Order().ToArray();

    private static async Task<JsonDocument> GetDocumentAsync(HttpResponseMessage response)
    {
        Assert.Equal(200, (int)response.StatusCode);

        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
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

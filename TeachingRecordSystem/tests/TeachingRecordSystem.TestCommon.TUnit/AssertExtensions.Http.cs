using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachingRecordSystem.TestCommon;

public static partial class AssertExtensions
{
    public static async Task<JsonDocument> JsonResponseAsync(HttpResponseMessage response, int expectedStatusCode = 200)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (expectedStatusCode != (int)response.StatusCode)
        {
            await Assert.That(response.StatusCode)
                .IsEqualTo((System.Net.HttpStatusCode)expectedStatusCode);
        }

        var contentType = response.Content.Headers.ContentType?.MediaType;
        if (contentType is not "application/json" and not "application/problem+json")
        {
            Assert.Fail($"Content type '{contentType}' is not a JSON content type");
        }

        var result = await response.Content.ReadFromJsonAsync<JsonDocument>(SerializerOptions);
        await Assert.That(result).IsNotNull();
        return result!;
    }

    public static async Task JsonResponseEqualsAsync(HttpResponseMessage response, object expected, int expectedStatusCode = 200)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(expected);

        var jsonDocument = await JsonResponseAsync(response, expectedStatusCode);

        JsonObjectEquals(expected, jsonDocument);
    }

    public static async Task JsonResponseIsErrorAsync(HttpResponseMessage response, int expectedErrorCode, int expectedStatusCode)
    {
        var problemDetails = await ResponseIsProblemDetailsAsync(response, expectedStatusCode);

        await Assert.That(problemDetails.Extensions)
            .IsNotNull()
            .And.Contains(kvp => kvp.Key == "errorCode");
        await Assert.That(problemDetails.Extensions!["errorCode"].GetInt32())
            .IsEqualTo(expectedErrorCode);
    }

    public static async Task JsonResponseHasValidationErrorForPropertyAsync(
        HttpResponseMessage response,
        string propertyName,
        string expectedError,
        int expectedStatusCode = 400)
    {
        var problemDetails = await ResponseIsProblemDetailsAsync(response, expectedStatusCode);

        await Assert.That(problemDetails.Extensions)
            .IsNotNull()
            .And.Contains(kvp => kvp.Key == propertyName);
        await Assert.That(problemDetails.Errors![propertyName].Single())
            .IsEqualTo(expectedError);
    }

    public static async Task JsonResponseHasValidationErrorsForPropertiesAsync(
        HttpResponseMessage response,
        IReadOnlyDictionary<string, string> expectedErrors,
        int expectedStatusCode = 400)
    {
        var problemDetails = await ResponseIsProblemDetailsAsync(response, expectedStatusCode);

        await Assert.That(problemDetails.Extensions).IsNotNull();

        using var _ = Assert.Multiple();
        foreach (var e in expectedErrors)
        {
            await Assert.That(problemDetails.Errors?[e.Key].Single()).IsEqualTo(e.Value);
        }
    }

    private static async Task<ProblemDetails> ResponseIsProblemDetailsAsync(HttpResponseMessage response, int expectedStatusCode)
    {
        ArgumentNullException.ThrowIfNull(response);

        using var _ = Assert.Multiple();

        await Assert.That((int)response.StatusCode).IsEqualTo(expectedStatusCode);
        await Assert.That(response.Content.Headers.ContentType?.MediaType).IsEqualTo("application/problem+json");

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        await Assert.That(problemDetails).IsNotNull();
        await Assert.That(problemDetails!.Status).IsEqualTo(expectedStatusCode);

        return problemDetails;
    }

    private class ProblemDetails
    {
        public string? Title { get; set; }
        public int Status { get; set; }
        [JsonConverter(typeof(ProblemDetailsErrorJsonConverter))]
        public IDictionary<string, string[]>? Errors { get; set; }
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? Extensions { get; set; }
    }

    private class ProblemDetailsErrorJsonConverter : JsonConverter<IDictionary<string, string[]>>
    {
        public override IDictionary<string, string[]>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dic = (Dictionary<string, string[]>)JsonSerializer.Deserialize(ref reader, typeToConvert, options)!;
            return new Dictionary<string, string[]>(dic, StringComparer.OrdinalIgnoreCase);
        }

        public override void Write(Utf8JsonWriter writer, IDictionary<string, string[]> value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }
}

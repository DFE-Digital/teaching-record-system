using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace TeachingRecordSystem.TestCommon;

public static partial class AssertEx
{
    public static async Task<JsonDocument> JsonResponse(HttpResponseMessage response, int expectedStatusCode = 200)
    {
        ArgumentNullException.ThrowIfNull(response);

        Assert.Equal(expectedStatusCode, (int)response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var result = await response.Content.ReadFromJsonAsync<JsonDocument>(SerializerOptions);
        Assert.NotNull(result);
        return result!;
    }

    public static async Task JsonResponseEquals(HttpResponseMessage response, object expected, int expectedStatusCode = 200)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(expected);

        var jsonDocument = await JsonResponse(response, expectedStatusCode);

        JsonObjectEquals(expected, jsonDocument);
    }

    public static async Task JsonResponseIsError(HttpResponseMessage response, int expectedErrorCode, int expectedStatusCode)
    {
        var problemDetails = await ResponseIsProblemDetails(response, expectedStatusCode);

        Assert.NotNull(problemDetails.Extensions);
        Assert.Contains(problemDetails.Extensions, kvp => kvp.Key == "errorCode");
        Assert.Equal(expectedErrorCode, problemDetails.Extensions?["errorCode"].GetInt32());
    }

    public static async Task JsonResponseHasValidationErrorForProperty(
        HttpResponseMessage response,
        string propertyName,
        string expectedError,
        int expectedStatusCode = 400)
    {
        var problemDetails = await ResponseIsProblemDetails(response, expectedStatusCode);

        Assert.NotNull(problemDetails.Extensions);
        Assert.Equal(expectedError, problemDetails.Errors?[propertyName].Single());
    }

    public static async Task JsonResponseHasValidationErrorsForProperties(
        HttpResponseMessage response,
        IReadOnlyDictionary<string, string> expectedErrors,
        int expectedStatusCode = 400)
    {
        var problemDetails = await ResponseIsProblemDetails(response, expectedStatusCode);

        Assert.NotNull(problemDetails.Extensions);

        foreach (var e in expectedErrors)
        {
            Assert.Equal(e.Value, problemDetails.Errors?[e.Key].Single());
        }
    }

    private static async Task<ProblemDetails> ResponseIsProblemDetails(HttpResponseMessage response, int expectedStatusCode)
    {
        ArgumentNullException.ThrowIfNull(response);

        Assert.Equal(expectedStatusCode, (int)response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal(expectedStatusCode, problemDetails!.Status);

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

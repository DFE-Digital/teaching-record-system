using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DqtApi.TestCommon
{
    public static partial class AssertEx
    {
        public static async Task<T> JsonResponse<T>(HttpResponseMessage response, int expectedStatusCode = StatusCodes.Status200OK)
        {
            if (response is null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            Assert.Equal(expectedStatusCode, (int)response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static Task<dynamic> JsonResponse(HttpResponseMessage response, int expectedStatusCode = StatusCodes.Status200OK) =>
            JsonResponse<dynamic>(response, expectedStatusCode);

        public static async Task JsonResponseEquals(HttpResponseMessage response, object expected, int expectedStatusCode = StatusCodes.Status200OK)
        {
            var jsonResponse = await JsonResponse<JObject>(response, expectedStatusCode);
            JsonObjectEquals(JToken.FromObject(expected), jsonResponse);
        }

        public static async Task ResponseIsError(HttpResponseMessage response, int errorCode, int expectedStatusCode)
        {
            var problemDetails = await ResponseIsProblemDetails(response, expectedStatusCode);

            Assert.Contains(problemDetails.Extensions, kvp => kvp.Key == "errorCode");
            Assert.Equal(errorCode, problemDetails.Extensions["errorCode"].ToObject<int>());
        }

        public static async Task ResponseIsValidationErrorForProperty(
            HttpResponseMessage response,
            string propertyName,
            string expectedError,
            int expectedStatusCode = 400)
        {
            var problemDetails = await ResponseIsProblemDetails(response, expectedStatusCode);

            Assert.Equal(expectedError, problemDetails.Errors[propertyName].Single());
        }

        private static async Task<ProblemDetails> ResponseIsProblemDetails(HttpResponseMessage response, int expectedStatusCode)
        {
            if (response is null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            Assert.Equal(expectedStatusCode, (int)response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType.MediaType);

            var json = await response.Content.ReadAsStringAsync();
            var problemDetails = JsonConvert.DeserializeObject<ProblemDetails>(json);
            Assert.Equal(expectedStatusCode, problemDetails.Status);

            return problemDetails;
        }

        private class ProblemDetails
        {
            public string Title { get; set; }
            public int Status { get; set; }
            [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<string[]>))]
            public Dictionary<string, string[]> Errors { get; set; }
            [JsonExtensionData]
            public IDictionary<string, JToken> Extensions { get; set; }
        }

        private class CaseInsensitiveDictionaryConverter<T> : JsonConverter
        {
            public override bool CanConvert(Type objectType) =>
                objectType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<string, T>));

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                {
                    return null;
                }

                var dictionary = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
                serializer.Populate(reader, dictionary);
                return dictionary;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotSupportedException();
            }
        }
    }
}

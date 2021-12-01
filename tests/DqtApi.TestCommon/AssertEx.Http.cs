using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
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

        public static async Task ResponseIsProblemDetails(HttpResponseMessage response, string expectedTitle, int expectedStatusCode = 400)
        {
            if (response is null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            Assert.Equal(expectedStatusCode, (int)response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType.MediaType);

            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.Equal(expectedStatusCode, problemDetails.Status);
            Assert.Equal(expectedTitle, problemDetails.Title);
        }

        private class ProblemDetails
        {
            [JsonPropertyName("title")]
            public string Title { get; set; }
            [JsonPropertyName("status")]
            public int Status { get; set; }
        }
    }
}

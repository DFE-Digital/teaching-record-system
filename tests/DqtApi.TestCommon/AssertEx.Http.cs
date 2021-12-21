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

        public static async Task ResponseIsProblemDetails(HttpResponseMessage response, string expectedError, string propertyName, int expectedStatusCode = 400)
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
            Assert.Equal(expectedError, problemDetails.Errors[propertyName].Single());
        }

        private class ProblemDetails
        {
            public string Title { get; set; }
            public int Status { get; set; }
            public Dictionary<string, string[]> Errors { get; set; }
        }
    }
}

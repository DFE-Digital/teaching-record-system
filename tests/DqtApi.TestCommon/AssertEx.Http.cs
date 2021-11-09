using System;
using System.Net.Http;
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
    }
}

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace DqtApi.FunctionalTests.Endpoints
{
    public class SwaggerTests : IAssemblyFixture<ApiFixture>
    {
        public SwaggerTests(ApiFixture apiFixture)
        {
            HttpClient = apiFixture.HttpClient;
        }

        public HttpClient HttpClient { get; }

        [Theory]
        [InlineData("v1")]
        [InlineData("v2")]
        public async Task GetSwaggerDoc(string version)
        {
            var response = await HttpClient.GetAsync($"swagger/{version}/swagger.json");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);

            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(json.SelectToken("openapi"));
        }
    }
}

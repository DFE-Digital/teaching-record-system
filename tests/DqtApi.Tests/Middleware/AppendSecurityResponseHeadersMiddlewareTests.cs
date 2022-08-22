using System.Net.Http;
using Xunit;

namespace DqtApi.Tests.Middleware
{
    public class AppendSecurityResponseHeadersMiddlewareTests : ApiTestBase
    {
        public AppendSecurityResponseHeadersMiddlewareTests(ApiFixture apiFixture) : base(apiFixture)
        {
        }


        [Fact]
        public async Task Given_a_request_response_contains_correct_headers()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/v2/itt-providers");

            // Act
            var response = await HttpClient.SendAsync(request);

            //Assert
            Assert.Contains("DENY", response.Headers.GetValues("X-Frame-Options"));
            Assert.Contains("1; mode=block", response.Headers.GetValues("X-Xss-Protection"));
            Assert.Contains("nosniff", response.Headers.GetValues("X-Content-Type-Options"));
            Assert.Contains("none", response.Headers.GetValues("X-Permitted-Cross-Domain-Policies"));
            Assert.Contains("no-referrer", response.Headers.GetValues("Referrer-Policy"));
            Assert.Contains("default-src 'self'", response.Headers.GetValues("Content-Security-Policy"));
        }
    }
}

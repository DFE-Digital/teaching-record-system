using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DqtApi.Tests
{
    [Collection("Api")]
    public abstract class ApiTestBase : IAsyncLifetime, IDisposable
    {
        protected ApiTestBase(ApiFixture apiFixture)
        {
            ApiFixture = apiFixture;

            HttpClient = apiFixture.CreateClient();
            HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "tests");

            apiFixture.ResetMocks();
        }

        public ApiFixture ApiFixture { get; }

        public HttpClient HttpClient { get; }

        public virtual void Dispose()
        {
        }

        public virtual Task DisposeAsync() => Task.CompletedTask;

        public virtual Task InitializeAsync() => Task.CompletedTask;
    }
}

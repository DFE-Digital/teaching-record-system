using System;
using System.Net.Http;
using System.Threading.Tasks;
using DqtApi.DataStore.Sql;
using Microsoft.Extensions.DependencyInjection;
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

        public virtual Task InitializeAsync() => ApiFixture.DbHelper.ClearData();

        public virtual async Task<T> WithDbContext<T>(Func<DqtContext, Task<T>> action)
        {
            await using var scope = ApiFixture.Services.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DqtContext>();
            return await action(dbContext);
        }

        public virtual Task WithDbContext(Func<DqtContext, Task> action) =>
            WithDbContext(async dbContext =>
            {
                await action(dbContext);
                return 0;
            });
    }
}

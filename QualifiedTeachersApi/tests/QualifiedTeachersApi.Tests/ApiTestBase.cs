using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using QualifiedTeachersApi.DataStore.Sql;
using QualifiedTeachersApi.Json;
using Xunit;

namespace QualifiedTeachersApi.Tests;

[Collection("Api")]
public abstract class ApiTestBase : IAsyncLifetime, IDisposable
{
    protected ApiTestBase(ApiFixture apiFixture)
    {
        ApiFixture = apiFixture;

        HttpClient = apiFixture.CreateClient();
        HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ClientId);

        apiFixture.ResetMocks();
    }

    public ApiFixture ApiFixture { get; }

    public string ClientId { get; } = "tests";

    public TestableClock Clock => (TestableClock)ApiFixture.Services.GetRequiredService<IClock>();

    public HttpClient HttpClient { get; }

    public DateTime UtcNow
    {
        get => Clock.UtcNow;
        set => Clock.UtcNow = value;
    }

    public JsonContent CreateJsonContent(object requestBody) =>
        JsonContent.Create(requestBody, options: new System.Text.Json.JsonSerializerOptions().AddConverters());

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

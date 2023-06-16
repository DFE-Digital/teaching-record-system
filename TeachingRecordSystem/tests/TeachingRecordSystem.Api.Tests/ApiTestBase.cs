using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using TeachingRecordSystem.Api.DataStore.Sql;
using TeachingRecordSystem.Api.Infrastructure.Json;
using Xunit;

namespace TeachingRecordSystem.Api.Tests;

[Collection("Api")]
public abstract class ApiTestBase : IAsyncLifetime, IDisposable
{
    protected ApiTestBase(ApiFixture apiFixture)
    {
        ApiFixture = apiFixture;

        {
            var key = apiFixture.Services.GetRequiredService<IConfiguration>()["ApiClients:tests:ApiKey:0"];
            HttpClientWithApiKey = apiFixture.CreateClient();
            HttpClientWithApiKey.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", key);
        }

        apiFixture.ResetMocks();
    }

    public ApiFixture ApiFixture { get; }

    public string ClientId { get; } = "tests";

    public TestableClock Clock => (TestableClock)ApiFixture.Services.GetRequiredService<IClock>();

    public HttpClient HttpClientWithApiKey { get; }

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

    public HttpClient GetHttpClientWithIdentityAccessToken(string trn, string scope = "dqt:read")
    {
        // The actual access tokens contain many more claims than this but these are the two we care about
        var subject = new ClaimsIdentity(new[]
        {
            new Claim("scope", scope),
            new Claim("trn", trn)
        });

        var jwtHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = subject,
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = ApiFixture.JwtSigningCredentials
        };

        var accessToken = jwtHandler.CreateEncodedJwt(tokenDescriptor);

        var httpClient = ApiFixture.CreateClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        return httpClient;
    }

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

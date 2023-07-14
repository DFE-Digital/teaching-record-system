using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TeachingRecordSystem.Api.Infrastructure.Json;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Certificates;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Dqt;

namespace TeachingRecordSystem.Api.Tests;

public abstract class ApiTestBase
{
    protected ApiTestBase(ApiFixture apiFixture)
    {
        ApiFixture = apiFixture;

        {
            var key = apiFixture.Services.GetRequiredService<IConfiguration>()["ApiClients:tests:ApiKey:0"];
            HttpClientWithApiKey = apiFixture.CreateClient();
            HttpClientWithApiKey.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", key);
        }
    }

    public ApiFixture ApiFixture { get; }

    public Mock<ICertificateGenerator> CertificateGenerator => Mock.Get(TestInfo.Current.TestServices.GetRequiredService<ICertificateGenerator>());

    public string ClientId { get; } = "tests";

    public Mock<IDataverseAdapter> DataverseAdapter => Mock.Get(TestInfo.Current.TestServices.GetRequiredService<IDataverseAdapter>());

    public TestableClock Clock => (TestableClock)ApiFixture.Services.GetRequiredService<IClock>();

    public Mock<IOptions<GetAnIdentityOptions>> GetAnIdentityOptions => Mock.Get(TestInfo.Current.TestServices.GetRequiredService<IOptions<GetAnIdentityOptions>>());

    public HttpClient HttpClientWithApiKey { get; }

    public Mock<IGetAnIdentityApiClient> IdentityApiClient => Mock.Get(TestInfo.Current.TestServices.GetRequiredService<IGetAnIdentityApiClient>());

    public JsonContent CreateJsonContent(object requestBody) =>
        JsonContent.Create(requestBody, options: new System.Text.Json.JsonSerializerOptions().AddConverters());

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

    public virtual async Task<T> WithDbContext<T>(Func<TrsDbContext, Task<T>> action)
    {
        await using var scope = ApiFixture.Services.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TrsDbContext>();
        return await action(dbContext);
    }

    public virtual Task WithDbContext(Func<TrsDbContext, Task> action) =>
        WithDbContext(async dbContext =>
        {
            await action(dbContext);
            return 0;
        });
}

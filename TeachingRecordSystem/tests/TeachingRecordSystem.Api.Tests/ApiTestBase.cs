using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FakeXrmEasy.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TeachingRecordSystem.Api.Infrastructure.Json;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.Certificates;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;

namespace TeachingRecordSystem.Api.Tests;

public abstract class ApiTestBase
{
    private readonly TestScopedServices _testServices;

    protected ApiTestBase(ApiFixture apiFixture)
    {
        ApiFixture = apiFixture;
        _testServices = TestScopedServices.Reset();

        {
            var key = apiFixture.Services.GetRequiredService<IConfiguration>()["ApiClients:tests:ApiKey:0"];
            HttpClientWithApiKey = apiFixture.CreateClient();
            HttpClientWithApiKey.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", key);
        }
    }

    public ApiFixture ApiFixture { get; }

    public Mock<ICertificateGenerator> CertificateGeneratorMock => _testServices.CertificateGeneratorMock;

    public string ClientId { get; } = "tests";

    public Mock<IDataverseAdapter> DataverseAdapterMock => _testServices.DataverseAdapterMock;

    public TestableClock Clock => _testServices.Clock;

    public Mock<IGetAnIdentityApiClient> GetAnIdentityApiClientMock => _testServices.GetAnIdentityApiClientMock;

    public IOptions<GetAnIdentityOptions> GetAnIdentityOptions => _testServices.GetAnIdentityOptions;

    public HttpClient HttpClientWithApiKey { get; }

    public TestData TestData => ApiFixture.Services.GetRequiredService<TestData>();

    public IXrmFakedContext XrmFakedContext => ApiFixture.Services.GetRequiredService<IXrmFakedContext>();

    public JsonContent CreateJsonContent(object requestBody) =>
        JsonContent.Create(requestBody, options: new System.Text.Json.JsonSerializerOptions().Configure());

    public HttpClient GetHttpClientWithIdentityAccessToken(string trn, string scope = "dqt:read")
    {
        // The actual access tokens contain many more claims than this but these are the two we care about
        var subject = new ClaimsIdentity(new[]
        {
            new Claim("scope", scope),
            new Claim("trn", trn)
        });

        var jwtHandler = new JwtSecurityTokenHandler();

        var signingCredentials = ApiFixture.JwtSigningCredentials;

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = subject,
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = signingCredentials
        };

        var accessToken = jwtHandler.CreateEncodedJwt(tokenDescriptor);

        var httpClient = ApiFixture.CreateClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        return httpClient;
    }

    public virtual async Task<T> WithDbContext<T>(Func<TrsDbContext, Task<T>> action)
    {
        var dbContextFactory = ApiFixture.Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await action(dbContext);
    }

    public virtual Task WithDbContext(Func<TrsDbContext, Task> action) =>
        WithDbContext(async dbContext =>
        {
            await action(dbContext);
            return 0;
        });
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FakeXrmEasy.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TeachingRecordSystem.Api.Infrastructure.Json;
using TeachingRecordSystem.Api.Tests.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.Certificates;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;

namespace TeachingRecordSystem.Api.Tests;

public abstract class TestBase
{
    private readonly TestScopedServices _testServices;

    protected TestBase(HostFixture hostFixture)
    {
        HostFixture = hostFixture;
        _testServices = TestScopedServices.Reset();
        SetCurrentApiClient(Array.Empty<string>());
    }

    public HostFixture HostFixture { get; }

    public Mock<ICertificateGenerator> CertificateGeneratorMock => _testServices.CertificateGeneratorMock;

    public string ClientId { get; } = "tests";

    public CrmQueryDispatcherSpy CrmQueryDispatcherSpy => _testServices.CrmQueryDispatcherSpy;

    public Mock<IDataverseAdapter> DataverseAdapterMock => _testServices.DataverseAdapterMock;

    public TestableClock Clock => _testServices.Clock;

    public Mock<IGetAnIdentityApiClient> GetAnIdentityApiClientMock => _testServices.GetAnIdentityApiClientMock;

    public TestData TestData => HostFixture.Services.GetRequiredService<TestData>();

    public IXrmFakedContext XrmFakedContext => HostFixture.Services.GetRequiredService<IXrmFakedContext>();

    public JsonContent CreateJsonContent(object requestBody) =>
        JsonContent.Create(requestBody, options: new System.Text.Json.JsonSerializerOptions().Configure());

    public virtual HttpClient GetHttpClientWithApiKey(string? version = null)
    {
        var client = HostFixture.CreateClient();
        client.DefaultRequestHeaders.Add("X-Use-CurrentClientIdProvider", "true");  // Signal for TestAuthenticationHandler to run

        if (version is not null)
        {
            client.DefaultRequestHeaders.Add(VersionRegistry.MinorVersionHeaderName, version);
        }

        return client;
    }

    public HttpClient GetHttpClientWithIdentityAccessToken(string trn, string scope = "dqt:read")
    {
        // The actual access tokens contain many more claims than this but these are the two we care about
        var subject = new ClaimsIdentity(new[]
        {
            new Claim("scope", scope),
            new Claim("trn", trn)
        });

        var jwtHandler = new JwtSecurityTokenHandler();

        var signingCredentials = HostFixture.JwtSigningCredentials;

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = subject,
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = signingCredentials
        };

        var accessToken = jwtHandler.CreateEncodedJwt(tokenDescriptor);

        var httpClient = HostFixture.CreateClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        return httpClient;
    }

    protected void SetCurrentApiClient(IEnumerable<string> roles, string clientId = "tests")
    {
        var currentUserProvider = HostFixture.Services.GetRequiredService<CurrentApiClientProvider>();
        currentUserProvider.CurrentApiClientId = clientId;
        currentUserProvider.Roles = roles.ToArray();
    }

    public virtual async Task<T> WithDbContext<T>(Func<TrsDbContext, Task<T>> action)
    {
        var dbContextFactory = HostFixture.Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();
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

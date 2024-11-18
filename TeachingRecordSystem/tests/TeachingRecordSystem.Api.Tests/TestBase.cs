using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using FakeXrmEasy.Abstractions;
using Microsoft.IdentityModel.Tokens;
using TeachingRecordSystem.Api.Tests.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Infrastructure.Json;
using TeachingRecordSystem.Core.Services.Certificates;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;

namespace TeachingRecordSystem.Api.Tests;

public abstract class TestBase
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new JsonStringEnumConverter(),
        },
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        {
            Modifiers =
            {
                Modifiers.OptionProperties
            }
        }
    };

    private readonly TestScopedServices _testServices;

    protected TestBase(HostFixture hostFixture)
    {
        HostFixture = hostFixture;
        _testServices = TestScopedServices.Reset();
        SetCurrentApiClient([]);
    }

    public HostFixture HostFixture { get; }

    public Mock<ICertificateGenerator> CertificateGeneratorMock => _testServices.CertificateGeneratorMock;

    public Guid DefaultApplicationUserId => HostFixture.DefaultApplicationUserId;

    public Guid ApplicationUserId { get; } = HostFixture.DefaultApplicationUserId;

    public CrmQueryDispatcherSpy CrmQueryDispatcherSpy => _testServices.CrmQueryDispatcherSpy;

    public Mock<IDataverseAdapter> DataverseAdapterMock => _testServices.DataverseAdapterMock;

    public TestableClock Clock => _testServices.Clock;

    public Mock<IGetAnIdentityApiClient> GetAnIdentityApiClientMock => _testServices.GetAnIdentityApiClientMock;

    public ReferenceDataCache ReferenceDataCache => HostFixture.Services.GetRequiredService<ReferenceDataCache>();

    public TestData TestData => HostFixture.Services.GetRequiredService<TestData>();

    public IXrmFakedContext XrmFakedContext => HostFixture.Services.GetRequiredService<IXrmFakedContext>();

    public JsonContent CreateJsonContent(object requestBody) =>
        JsonContent.Create(requestBody, options: _jsonSerializerOptions);

    public virtual HttpClient GetHttpClient(string? version = null)
    {
        var client = HostFixture.CreateClient();

        if (version is not null)
        {
            client.DefaultRequestHeaders.Add(VersionRegistry.MinorVersionHeaderName, version);
        }

        return client;
    }

    public virtual HttpClient GetHttpClientWithApiKey(string? version = null)
    {
        var client = GetHttpClient(version);
        client.DefaultRequestHeaders.Add("X-Use-CurrentClientIdProvider", "true");  // Signal for TestAuthenticationHandler to run
        return client;
    }

    public HttpClient GetHttpClientWithIdentityAccessToken(string trn, string scope = "dqt:read", string? version = null)
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

        if (version is not null)
        {
            httpClient.DefaultRequestHeaders.Add(VersionRegistry.MinorVersionHeaderName, version);
        }

        return httpClient;
    }

    protected void SetCurrentApiClient(IEnumerable<string> roles, Guid? applicationUserId = null)
    {
        var currentUserProvider = HostFixture.Services.GetRequiredService<CurrentApiClientProvider>();
        currentUserProvider.CurrentApiUserId = applicationUserId ?? DefaultApplicationUserId;
        currentUserProvider.Roles = roles.ToArray();
    }

    public virtual async Task<T> WithDbContextAsync<T>(Func<TrsDbContext, Task<T>> action)
    {
        var dbContextFactory = HostFixture.Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await action(dbContext);
    }

    public virtual Task WithDbContextAsync(Func<TrsDbContext, Task> action) =>
        WithDbContextAsync(async dbContext =>
        {
            await action(dbContext);
            return 0;
        });
}

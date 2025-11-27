using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.IdentityModel.Tokens;
using TeachingRecordSystem.Api.IntegrationTests.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Infrastructure.Json;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.TestCommon.Infrastructure;

namespace TeachingRecordSystem.Api.IntegrationTests;

public abstract class TestBase
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new JsonStringEnumConverter()
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
        _testServices = TestScopedServices.Reset(hostFixture.Services);
        SetCurrentApiClient([]);
    }

    protected HostFixture HostFixture { get; }

    protected IDbContextFactory<TrsDbContext> DbContextFactory => HostFixture.Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    protected DbHelper DbHelper => HostFixture.Services.GetRequiredService<DbHelper>();

    protected Guid DefaultApplicationUserId => HostFixture.DefaultApplicationUserId;

    protected Guid ApplicationUserId { get; } = HostFixture.DefaultApplicationUserId;

    protected TestableClock Clock => _testServices.Clock;

    protected Mock<IGetAnIdentityApiClient> GetAnIdentityApiClientMock => _testServices.GetAnIdentityApiClientMock;

    protected ReferenceDataCache ReferenceDataCache => HostFixture.Services.GetRequiredService<ReferenceDataCache>();

    protected TestData TestData => HostFixture.Services.GetRequiredService<TestData>();

    protected TestableFeatureProvider FeatureProvider => _testServices.FeatureProvider;

    protected JsonContent CreateJsonContent(object requestBody) =>
        JsonContent.Create(requestBody, options: _jsonSerializerOptions);

    protected Mock<IFileService> BlobStorageFileService => _testServices.BlobStorageFileServiceMock;

    protected virtual HttpClient GetHttpClient(string? version = null)
    {
        var client = HostFixture.CreateClient(new()
        {
            AllowAutoRedirect = false
        });

        if (version is not null)
        {
            client.DefaultRequestHeaders.Add(VersionRegistry.MinorVersionHeaderName, version);
        }

        return client;
    }

    protected virtual HttpClient GetHttpClientWithApiKey(string? version = null)
    {
        var client = GetHttpClient(version);
        client.DefaultRequestHeaders.Add("X-Use-CurrentClientIdProvider", "true");  // Signal for TestAuthenticationHandler to run
        return client;
    }

    protected HttpClient GetHttpClientWithIdentityAccessToken(string trn, string scope = "dqt:read", string? version = null)
    {
        // The actual access tokens contain many more claims than this but these are the two we care about
        var subject = new ClaimsIdentity([
            new Claim("scope", scope),
            new Claim("trn", trn)
        ]);

        var jwtHandler = new JwtSecurityTokenHandler { MapInboundClaims = false };

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

    protected Task<T> WithDbContextAsync<T>(Func<TrsDbContext, Task<T>> action) =>
        DbContextFactory.WithDbContextAsync(action);

    protected Task WithDbContextAsync(Func<TrsDbContext, Task> action) =>
        DbContextFactory.WithDbContextAsync(action);
}

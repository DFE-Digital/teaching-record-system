using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.Tokens;
using TeachingRecordSystem.Api.IntegrationTests.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Infrastructure.Json;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.TestCommon.Database;
using TeachingRecordSystem.TestCommon.Infrastructure;

namespace TeachingRecordSystem.Api.IntegrationTests;

public abstract class TestBase : IAsyncLifetime
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
    private TestDatabaseLease? _databaseLease;
    private IDisposable? _serviceScope;

    protected TestBase(HostFixture hostFixture)
    {
        HostFixture = hostFixture;
        _testServices = TestScopedServices.Reset(hostFixture.Services);
        SetCurrentApiClient([]);
    }

    protected HostFixture HostFixture { get; }

    protected string DatabaseName =>
        _databaseLease?.DatabaseName ?? throw new InvalidOperationException("No database has been leased.");

    public virtual async ValueTask InitializeAsync()
    {
        _databaseLease = await TestDatabases.AcquireAsync(TestContext.Current.CancellationToken);
        _serviceScope = TestServiceScope.Push(HostFixture.RootServices);
    }

    public virtual async ValueTask DisposeAsync()
    {
        _serviceScope?.Dispose();
        _serviceScope = null;

        if (_databaseLease is null)
        {
            return;
        }

        // Keep a failing test's data so it can be inspected: psql -d <name>
        if (TestContext.Current.TestState?.Result == TestResult.Failed)
        {
            TestContext.Current.SendDiagnosticMessage($"Retained test database '{_databaseLease.DatabaseName}'.");
            _databaseLease.Retain();
        }

        await _databaseLease.DisposeAsync();
        _databaseLease = null;
    }

    protected IDbContextFactory<TrsDbContext> DbContextFactory => HostFixture.Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    protected Guid DefaultApplicationUserId => HostFixture.DefaultApplicationUserId;

    protected Guid ApplicationUserId { get; } = HostFixture.DefaultApplicationUserId;

    protected FakeTimeProvider TimeProvider => _testServices.TimeProvider;

    protected ReferenceDataCache ReferenceDataCache => HostFixture.Services.GetRequiredService<ReferenceDataCache>();

    protected TestData TestData => HostFixture.Services.GetRequiredService<TestData>();

    protected TestableFeatureProvider FeatureProvider => _testServices.FeatureProvider;

    protected JsonContent CreateJsonContent(object requestBody) =>
        JsonContent.Create(requestBody, options: _jsonSerializerOptions);

    protected Mock<IFileService> BlobStorageFileService => _testServices.BlobStorageFileServiceMock;

    protected HttpClient GetHttpClient(string? version = null)
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

    protected HttpClient GetHttpClientWithApiKey(string? version = null)
    {
        var client = GetHttpClient(version);
        client.DefaultRequestHeaders.Add("X-Use-CurrentClientIdProvider", "true");  // Signal for TestAuthenticationHandler to run
        return client;
    }

    protected HttpClient GetHttpClientWithAuthorizeAccessToken(string trn, string version)
    {
        Claim[] claims = [
            new("scope", "teaching_record"),
            new(AuthorizeAccessClaimTypes.Trn, trn),
            new(AuthorizeAccessClaimTypes.TrsApplicationUserId, HostFixture.DefaultApplicationUserId.ToString())
        ];

        return GetHttpClientWithJwtAccessToken(claims, version);
    }

    protected HttpClient GetHttpClientWithAuthorizeAccessTokenForTrnRequest(Guid applicationUserId, string trnRequestId, string version)
    {
        Claim[] claims = [
            new("scope", "teaching_record"),
            new(AuthorizeAccessClaimTypes.TrnRequestId, trnRequestId),
            new(AuthorizeAccessClaimTypes.TrsApplicationUserId, applicationUserId.ToString())
        ];

        return GetHttpClientWithJwtAccessToken(claims, version);
    }

    private HttpClient GetHttpClientWithJwtAccessToken(IEnumerable<Claim> claims, string? version = null)
    {
        var subject = new ClaimsIdentity(claims);

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

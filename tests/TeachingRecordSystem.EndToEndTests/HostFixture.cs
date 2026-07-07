using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Playwright;
using OpenIddict.Server.AspNetCore;
using TeachingRecordSystem.AuthorizeAccess;
using TeachingRecordSystem.Core.ApiSchema;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.EventHandlers;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.Core.Services.Webhooks;
using TeachingRecordSystem.EndToEndTests;
using TeachingRecordSystem.EndToEndTests.Infrastructure.Security;
using TeachingRecordSystem.EndToEndTests.Infrastructure.Webhooks;
using TeachingRecordSystem.TestCommon.Infrastructure;

[assembly: AssemblyFixture(typeof(HostFixture))]

namespace TeachingRecordSystem.EndToEndTests;

public sealed class HostFixture : InitializeDbFixture
{
    public const string FakeOneLoginAuthenticationScheme = "FakeOneLogin";
    public const string DeferredFakeOneLoginAuthenticationScheme = "DeferredFakeOneLogin";

    private const int ApiPort = 5900;
    private const int AuthorizeAccessPort = 55649;
    private const int SupportUiPort = 5901;

    public static Guid DeferredRecordMatchingPolicyApplicationUserId { get; } = new("D498A7AE-27AC-4D8B-9B5B-0FCD15028165");

    private readonly ApiWebApplicationFactory _apiWebApplicationFactory;
    private readonly AuthorizeAccessWebApplicationFactory _authorizeAccessWebApplicationFactory;
    private readonly SupportUiWebApplicationFactory _supportUiWebApplicationFactory;

    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;

    private readonly WebhookReceiver _webhookReceiver;

    public HostFixture()
    {
        _apiWebApplicationFactory = new(this);
        _authorizeAccessWebApplicationFactory = new(this);
        _supportUiWebApplicationFactory = new();

        _webhookReceiver = new();

        TimeProvider = TimeProvider.System;

        TestData = new(
            DbHelper.Instance.DbContextFactory,
            new ReferenceDataCache(DbHelper.Instance.DbContextFactory),
            this.TimeProvider);

        using (var rsa = RSA.Create())
        {
            JwtSigningCredentials = new SigningCredentials(
                new RsaSecurityKey(rsa.ExportParameters(includePrivateParameters: true)),
                SecurityAlgorithms.RsaSha256);
        }

        WebhookMessageRecorder = _webhookReceiver.WebhookMessageRecorder;
    }

    public static string ApiBaseUrl => $"http://localhost:{ApiPort}";
    public static string AuthorizeAccessBaseUrl => $"http://localhost:{AuthorizeAccessPort}";
    public static string SupportUiBaseUrl => $"http://localhost:{SupportUiPort}";

    public IServiceProvider ApiHostServices => _apiWebApplicationFactory.Services;
    public IServiceProvider AuthorizeAccessHostServices => _authorizeAccessWebApplicationFactory.Services;
    public IServiceProvider SupportUiHostServices => _supportUiWebApplicationFactory.Services;

    public SigningCredentials JwtSigningCredentials { get; }

    public IDbContextFactory<TrsDbContext> DbContextFactory => DbHelper.DbContextFactory;
    public TimeProvider TimeProvider { get; }
    public TestData TestData { get; }
    public WebhookMessageRecorder WebhookMessageRecorder { get; }

    public override async ValueTask InitializeAsync()
    {
        _apiWebApplicationFactory.StartServer();
        _authorizeAccessWebApplicationFactory.StartServer();
        //_supportUiWebApplicationFactory.StartServer();

        _playwright = await Playwright.CreateAsync();

        var browserOptions = new BrowserTypeLaunchOptions()
        {
            Timeout = 10000,
            Args = ["--start-maximized"]
        };

        if (Debugger.IsAttached)
        {
            browserOptions.Headless = false;
            browserOptions.SlowMo = 250;
        }

        var browserType = OperatingSystem.IsMacOS() ? _playwright.Webkit : _playwright.Chromium;
        _browser = await browserType.LaunchAsync(browserOptions);

        var dbHelper = DbHelper.Instance;
        await dbHelper.InitializeAsync();
        await dbHelper.ClearDataAsync();
        await AddTestAppToApplicationUsers();
        await AddWebhookReceiverEndpoint();
    }

    public override async ValueTask DisposeAsync()
    {
        await _apiWebApplicationFactory.DisposeAsync();
        await _authorizeAccessWebApplicationFactory.DisposeAsync();
        await _supportUiWebApplicationFactory.DisposeAsync();

        _webhookReceiver.Dispose();

        await base.DisposeAsync();
    }

    public Task<IBrowserContext> CreateBrowserContext() =>
        _browser.NewContextAsync(new()
        {
            ViewportSize = ViewportSize.NoViewport
        });

    public HttpClient GetHttpClientWithAuthorizeAccessTokenForTrnRequest(
        Guid applicationUserId,
        string trnRequestId,
        string? version)
    {
        Claim[] claims = [
            new("scope", "teaching_record"),
            new(AuthorizeAccessClaimTypes.TrnRequestId, trnRequestId),
            new(AuthorizeAccessClaimTypes.TrsApplicationUserId, applicationUserId.ToString())
        ];

        var subject = new ClaimsIdentity(claims);

        var jwtHandler = new JwtSecurityTokenHandler { MapInboundClaims = false };

        var signingCredentials = JwtSigningCredentials;

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = subject,
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = signingCredentials
        };

        var accessToken = jwtHandler.CreateEncodedJwt(tokenDescriptor);

        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(ApiBaseUrl);
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        if (version is not null)
        {
            httpClient.DefaultRequestHeaders.Add(VersionRegistry.MinorVersionHeaderName, version);
        }

        return httpClient;
    }

    private async Task AddTestAppToApplicationUsers()
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();

        // Add the default test app with Required record matching policy
        dbContext.ApplicationUsers.Add(new Core.DataStore.Postgres.Models.ApplicationUser()
        {
            UserId = Guid.NewGuid(),
            Name = "Test App",
            IsOidcClient = true,
            ClientId = TestAppConfiguration.ClientId,
            ClientSecret = TestAppConfiguration.ClientSecret,
            RedirectUris = [AuthorizeAccessBaseUrl + TestAppConfiguration.RedirectUriPath],
            PostLogoutRedirectUris = [AuthorizeAccessBaseUrl + TestAppConfiguration.PostLogoutRedirectUriPath],
            OneLoginAuthenticationSchemeName = FakeOneLoginAuthenticationScheme,
            RecordMatchingPolicy = RecordMatchingPolicy.Required
        });

        // Add the deferred test app with Deferred record matching policy
        dbContext.ApplicationUsers.Add(new Core.DataStore.Postgres.Models.ApplicationUser()
        {
            UserId = DeferredRecordMatchingPolicyApplicationUserId,
            Name = "Test App (Deferred Matching)",
            IsOidcClient = true,
            ClientId = DeferredTestAppConfiguration.ClientId,
            ClientSecret = DeferredTestAppConfiguration.ClientSecret,
            RedirectUris = [AuthorizeAccessBaseUrl + DeferredTestAppConfiguration.RedirectUriPath],
            PostLogoutRedirectUris = [AuthorizeAccessBaseUrl + DeferredTestAppConfiguration.PostLogoutRedirectUriPath],
            OneLoginAuthenticationSchemeName = DeferredFakeOneLoginAuthenticationScheme,
            RecordMatchingPolicy = RecordMatchingPolicy.Deferred
        });

        await dbContext.SaveChangesAsync();
    }

    private async Task AddWebhookReceiverEndpoint()
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();

        dbContext.WebhookEndpoints.Add(new Core.DataStore.Postgres.Models.WebhookEndpoint
        {
            WebhookEndpointId = Guid.NewGuid(),
            ApplicationUserId = DeferredRecordMatchingPolicyApplicationUserId,
            Address = _webhookReceiver.FullyQualifiedEndpoint,
            ApiVersion = VersionRegistry.V3MinorVersions.V20260416,
            CloudEventTypes = ["trn_request.completed"],
            Enabled = true,
            CreatedOn = TimeProvider.UtcNow,
            UpdatedOn = TimeProvider.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services
            .AddSingleton<INotificationSender, NoopNotificationSender>()
            .AddSingleton<IBackgroundJobScheduler, ExecuteOnCommitBackgroundJobScheduler>();

        services.Configure<WebhookOptions>(options =>
        {
            options.CanonicalDomain = "https://dummy";
            options.SigningKeyId = "key";
            options.Keys =
            [
                new WebhookOptionsKey()
                {
                    KeyId = _webhookReceiver.KeyId,
                    CertificatePem = _webhookReceiver.Certificate.ExportCertificatePem(),
                    PrivateKeyPem = _webhookReceiver.SigningKey.ExportECPrivateKeyPem()
                }
            ];
        });

        WebhookSender.Register(services);

        // Replace CreateWebhookMessages with SendWebhookMessagesEventHandler;
        // we want to dispatch webhook messages immediately instead of queueing them
        services.Remove(services.Single(sd => sd.ImplementationType == typeof(CreateWebhookMessages)));
        services.AddTransient<IEventHandler, SendWebhookMessagesEventHandler>();
    }

    private class ApiWebApplicationFactory : WebApplicationFactory<Api.Program>
    {
        private readonly HostFixture _hostFixture;

        public ApiWebApplicationFactory(HostFixture hostFixture)
        {
            _hostFixture = hostFixture;

            UseKestrel(ApiPort);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("EndToEndTests");

            var configuration = TestConfiguration.GetConfiguration();
            builder.UseConfiguration(configuration);

            builder.ConfigureServices((context, services) =>
            {
                // Replace authentication handlers with mechanisms we can control from tests
                services.Configure<AuthenticationOptions>(options =>
                {
                    //options.SchemeMap[ApiKeyAuthenticationHandler.AuthenticationScheme].HandlerType = typeof(TestApiKeyAuthenticationHandler);
                    options.SchemeMap["IdAccessToken"].HandlerType = typeof(SimpleJwtBearerAuthentication);
                    options.SchemeMap["AuthorizeAccessAccessToken"].HandlerType = typeof(SimpleJwtBearerAuthentication);
                });

                services.Configure<SimpleJwtBearerAuthenticationOptions>("IdAccessToken", o => o.IssuerSigningKey = _hostFixture.JwtSigningCredentials.Key);
                services.Configure<SimpleJwtBearerAuthenticationOptions>("AuthorizeAccessAccessToken", o => o.IssuerSigningKey = _hostFixture.JwtSigningCredentials.Key);

                _hostFixture.ConfigureServices(services);

                services
                    .AddSingleton(DbHelper.Instance)
                    .AddSingleton<TestData>();
            });
        }
    }

    private class AuthorizeAccessWebApplicationFactory : WebApplicationFactory<AuthorizeAccess.Program>
    {
        private readonly HostFixture _hostFixture;

        public AuthorizeAccessWebApplicationFactory(HostFixture hostFixture)
        {
            _hostFixture = hostFixture;

            UseKestrel(AuthorizeAccessPort);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("EndToEndTests");

            builder.UseStaticWebAssets();

            var configuration = TestConfiguration.GetConfiguration();
            builder.UseConfiguration(configuration);

            builder.ConfigureServices(services =>
            {
                services.Configure<AuthenticationOptions>(options =>
                {
                    options.AddScheme(FakeOneLoginAuthenticationScheme, b => b.HandlerType = typeof(FakeOneLoginHandler));
                    options.AddScheme(DeferredFakeOneLoginAuthenticationScheme, b => b.HandlerType = typeof(FakeOneLoginHandler));
                });

                services.Configure<OpenIdConnectOptions>(
                    TestAppConfiguration.AuthenticationSchemeName,
                    options =>
                    {
                        options.Authority = AuthorizeAccessBaseUrl;
                        options.RequireHttpsMetadata = false;
                    });

                services.Configure<OpenIdConnectOptions>(
                    DeferredTestAppConfiguration.AuthenticationSchemeName,
                    options =>
                    {
                        options.Authority = AuthorizeAccessBaseUrl;
                        options.RequireHttpsMetadata = false;
                    });

                services.Configure<OpenIddictServerAspNetCoreOptions>(options => options.DisableTransportSecurityRequirement = true);

                services.Configure<AuthorizeAccessOptions>(options =>
                {
                    using var rsa = RSA.Create(keySizeInBits: 2048);
                    var privateKeyPem = rsa.ExportRSAPrivateKeyPem();

                    options.OneLoginSigningKeys =
                    [
                        new AuthorizeAccessOptionsOneLoginSigningKey
                        {
                            KeyId = "test-key-1",
                            PrivateKeyPem = privateKeyPem
                        }
                    ];
                });

                _hostFixture.ConfigureServices(services);

                services
                    .AddSingleton(DbHelper.Instance)
                    .AddSingleton<TestData>()
                    .AddSingleton<OneLoginCurrentUserProvider>()
                    .AddSingleton(GetMockFileService())
                    .AddSingleton(GetMockSafeFileService());

                IFileService GetMockFileService()
                {
                    var fileService = new Mock<IFileService>();
                    fileService
                        .Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string?>(), null))
                        .ReturnsAsync(Guid.NewGuid());
                    fileService
                        .Setup(s => s.GetFileUrlAsync(It.IsAny<Guid>(), It.IsAny<TimeSpan>()))
                        .ReturnsAsync("https://fake.blob.core.windows.net/fake");
                    fileService
                        .Setup(s => s.OpenReadStreamAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(() => new MemoryStream(TestData.JpegImage));
                    return fileService.Object;
                }

                ISafeFileService GetMockSafeFileService()
                {
                    var safeFileService = new Mock<ISafeFileService>();
                    safeFileService
                        .Setup(s => s.TrySafeUploadAsync(
                            It.IsAny<Stream>(),
                            It.IsAny<string?>(),
                            out It.Ref<Guid>.IsAny,
                            null))
                        .Callback((Stream stream, string? contentType, out Guid fileId, Guid? fileIdOverride) =>
                        {
                            fileId = fileIdOverride ?? Guid.NewGuid();
                        })
                        .ReturnsAsync(true);
                    return safeFileService.Object;
                }
            });
        }
    }

    private class SupportUiWebApplicationFactory : WebApplicationFactory<SupportUi.Program>
    {
        public SupportUiWebApplicationFactory()
        {
            UseKestrel(SupportUiPort);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {

        }
    }
}

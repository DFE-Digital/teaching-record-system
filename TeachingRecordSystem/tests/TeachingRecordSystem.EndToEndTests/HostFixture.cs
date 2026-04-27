using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using OpenIddict.Server.AspNetCore;
using TeachingRecordSystem.AuthorizeAccess;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.EndToEndTests;
using TeachingRecordSystem.EndToEndTests.Infrastructure.Security;
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

    private readonly ApiWebApplicationFactory _apiWebApplicationFactory = new();
    private readonly AuthorizeAccessWebApplicationFactory _authorizeAccessWebApplicationFactory = new();
    private readonly SupportUiWebApplicationFactory _supportUiWebApplicationFactory = new();

    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;

    public HostFixture()
    {
        TimeProvider = TimeProvider.System;

        TestData = new(
            DbHelper.Instance.DbContextFactory,
            new ReferenceDataCache(DbHelper.Instance.DbContextFactory),
            this.TimeProvider);
    }

    public static string ApiBaseUrl => $"http://localhost:{ApiPort}";
    public static string AuthorizeAccessBaseUrl => $"http://localhost:{AuthorizeAccessPort}";
    public static string SupportUiBaseUrl => $"http://localhost:{SupportUiPort}";

    public IServiceProvider ApiHostServices => _apiWebApplicationFactory.Services;
    public IServiceProvider AuthorizeAccessHostServices => _authorizeAccessWebApplicationFactory.Services;
    public IServiceProvider SupportUiHostServices => _supportUiWebApplicationFactory.Services;

    public IDbContextFactory<TrsDbContext> DbContextFactory => DbHelper.DbContextFactory;
    public TimeProvider TimeProvider { get; }
    public TestData TestData { get; }

    public override async ValueTask InitializeAsync()
    {
        //_apiWebApplicationFactory.StartServer();
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
    }

    public override async ValueTask DisposeAsync()
    {
        await _apiWebApplicationFactory.DisposeAsync();
        await _authorizeAccessWebApplicationFactory.DisposeAsync();
        await _supportUiWebApplicationFactory.DisposeAsync();

        await base.DisposeAsync();
    }

    public Task<IBrowserContext> CreateBrowserContext() =>
        _browser.NewContextAsync(new()
        {
            ViewportSize = ViewportSize.NoViewport
        });

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
            UserId = Guid.NewGuid(),
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

    private class ApiWebApplicationFactory : WebApplicationFactory<Api.Program>
    {
        public ApiWebApplicationFactory()
        {
            UseKestrel(ApiPort);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
        }
    }

    private class AuthorizeAccessWebApplicationFactory : WebApplicationFactory<AuthorizeAccess.Program>
    {
        public AuthorizeAccessWebApplicationFactory()
        {
            UseKestrel(AuthorizeAccessPort);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("EndToEndTests");

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

                services
                    .AddSingleton(DbHelper.Instance)
                    .AddSingleton<TestData>()
                    .AddSingleton<OneLoginCurrentUserProvider>()
                    .AddSingleton(GetMockFileService())
                    .AddSingleton(GetMockSafeFileService())
                    .AddSingleton(Mock.Of<IGetAnIdentityApiClient>())
                    .AddSingleton<IBackgroundJobScheduler, ExecuteOnCommitBackgroundJobScheduler>();

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

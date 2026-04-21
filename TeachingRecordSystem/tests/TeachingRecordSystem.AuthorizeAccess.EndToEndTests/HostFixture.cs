using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Playwright;
using OpenIddict.Server.AspNetCore;
using TeachingRecordSystem.AuthorizeAccess.EndToEndTests;
using TeachingRecordSystem.AuthorizeAccess.EndToEndTests.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.TestCommon.Infrastructure;
using TeachingRecordSystem.UiTestCommon.Infrastructure.FormFlow;
using TeachingRecordSystem.WebCommon.FormFlow.State;

[assembly: AssemblyFixture(typeof(HostFixture))]

namespace TeachingRecordSystem.AuthorizeAccess.EndToEndTests;

public sealed class HostFixture : InitializeDbFixture
{
    private const int Port = 55649;
    public const string BaseUrl = "http://localhost:55649";
    public const string FakeOneLoginAuthenticationScheme = "FakeOneLogin";
    public const string DeferredFakeOneLoginAuthenticationScheme = "DeferredFakeOneLogin";

    private bool _initialized;
    private bool _disposed;
    private readonly ApiWebApplicationFactory _webApplicationFactory = new();
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public IConfiguration Configuration => Services.GetRequiredService<IConfiguration>();

    public IBrowser Browser
    {
        get
        {
            ThrowIfNotInitialized();
            return _browser;
        }
    }

    public IServiceProvider Services
    {
        get
        {
            ThrowIfNotInitialized();
            return _webApplicationFactory.Services;
        }
    }

    public Task<IBrowserContext> CreateBrowserContext()
    {
        ThrowIfNotInitialized();

        return _browser.NewContextAsync(new()
        {
            BaseURL = BaseUrl,
            ViewportSize = ViewportSize.NoViewport
        });
    }

    [MemberNotNull(nameof(_playwright), nameof(_browser), nameof(_webApplicationFactory))]
    private void ThrowIfNotInitialized()
    {
        if (!_initialized || _playwright is null || _browser is null || _webApplicationFactory is null)
        {
            throw new InvalidOperationException("Fixture has not been initialized");
        }
    }

    private async Task AddTestAppToApplicationUsers()
    {
        await using var dbContext = await Services.GetRequiredService<IDbContextFactory<TrsDbContext>>().CreateDbContextAsync();

        // Add the default test app with Required record matching policy
        dbContext.ApplicationUsers.Add(new Core.DataStore.Postgres.Models.ApplicationUser()
        {
            UserId = Guid.NewGuid(),
            Name = "Test App",
            IsOidcClient = true,
            ClientId = TestAppConfiguration.ClientId,
            ClientSecret = TestAppConfiguration.ClientSecret,
            RedirectUris = [BaseUrl + TestAppConfiguration.RedirectUriPath],
            PostLogoutRedirectUris = [BaseUrl + TestAppConfiguration.PostLogoutRedirectUriPath],
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
            RedirectUris = [BaseUrl + DeferredTestAppConfiguration.RedirectUriPath],
            PostLogoutRedirectUris = [BaseUrl + DeferredTestAppConfiguration.PostLogoutRedirectUriPath],
            OneLoginAuthenticationSchemeName = DeferredFakeOneLoginAuthenticationScheme,
            RecordMatchingPolicy = RecordMatchingPolicy.Deferred
        });

        await dbContext.SaveChangesAsync();
    }

    public override async ValueTask InitializeAsync()
    {
        _webApplicationFactory.StartServer();

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

        _initialized = true;

        var dbHelper = Services.GetRequiredService<DbHelper>();
        await dbHelper.InitializeAsync();
        await dbHelper.ClearDataAsync();
        await AddTestAppToApplicationUsers();
    }

    public override async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_browser is not null)
        {
            await _browser.DisposeAsync();
        }

        _playwright?.Dispose();

        await _webApplicationFactory.DisposeAsync();

        await base.DisposeAsync();
    }

    private class ApiWebApplicationFactory : WebApplicationFactory<Program>
    {
        public ApiWebApplicationFactory()
        {
            UseKestrel(Port);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("EndToEndTests");

            var configuration = TestConfiguration.GetConfiguration();
            builder.UseConfiguration(configuration);

            builder.ConfigureServices((context, services) =>
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
                        options.Authority = BaseUrl;
                        options.RequireHttpsMetadata = false;
                    });

                services.Configure<OpenIdConnectOptions>(
                    DeferredTestAppConfiguration.AuthenticationSchemeName,
                    options =>
                    {
                        options.Authority = BaseUrl;
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
                    .AddSingleton<IUserInstanceStateProvider, InMemoryInstanceStateProvider>()
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
}

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Playwright;
using OpenIddict.Server.AspNetCore;
using TeachingRecordSystem.AuthorizeAccess.EndToEndTests.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.TrsDataSync;
using TeachingRecordSystem.UiCommon.FormFlow.State;
using TeachingRecordSystem.UiTestCommon.Infrastructure.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.EndToEndTests;

public sealed class HostFixture(IConfiguration configuration) : IAsyncDisposable, IStartupTask
{
    public const string BaseUrl = "http://localhost:55649";
    public const string FakeOneLoginAuthenticationScheme = "FakeOneLogin";

    private bool _initialized = false;
    private bool _disposed = false;
    private Host<Program>? _host;
    private IPlaywright? _playwright;
    private IBrowser? _browser;

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
            return _host.Services;
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

    private Host<Program> CreateHost() =>
        Host<Program>.CreateHost(
            BaseUrl,
            builder =>
            {
                builder.UseConfiguration(configuration);

                builder.ConfigureServices((context, services) =>
                {
                    DbHelper.ConfigureDbServices(services, context.Configuration.GetRequiredConnectionString("DefaultConnection"));

                    services.AddDbContext<IdDbContext>(options => options.UseInMemoryDatabase("TeacherAuthId"), contextLifetime: ServiceLifetime.Transient);

                    services.Configure<AuthenticationOptions>(options =>
                    {
                        options.AddScheme(FakeOneLoginAuthenticationScheme, b => b.HandlerType = typeof(FakeOneLoginHandler));
                    });

                    services.Configure<OpenIdConnectOptions>(
                        TestAppConfiguration.AuthenticationSchemeName,
                        options =>
                        {
                            options.Authority = BaseUrl;
                            options.RequireHttpsMetadata = false;
                        });

                    services.Configure<OpenIddictServerAspNetCoreOptions>(options => options.DisableTransportSecurityRequirement = true);

                    services.AddSingleton<OneLoginCurrentUserProvider>();
                    services.AddSingleton<TestData>(
                        sp => ActivatorUtilities.CreateInstance<TestData>(sp, TestDataSyncConfiguration.Sync(sp.GetRequiredService<TrsDataSyncHelper>())));
                    services.AddFakeXrm();
                    services.AddSingleton<FakeTrnGenerator>();
                    services.AddSingleton<TrsDataSyncHelper>();
                    services.AddSingleton<IUserInstanceStateProvider, InMemoryInstanceStateProvider>();
                    services.AddSingleton(GetMockFileService());

                    IFileService GetMockFileService()
                    {
                        var fileService = new Mock<IFileService>();
                        fileService
                            .Setup(s => s.UploadFile(It.IsAny<Stream>(), It.IsAny<string?>()))
                            .ReturnsAsync(Guid.NewGuid());
                        fileService
                            .Setup(s => s.GetFileUrl(It.IsAny<Guid>(), It.IsAny<TimeSpan>()))
                            .ReturnsAsync("https://fake.blob.core.windows.net/fake");
                        fileService
                            .Setup(s => s.OpenReadStream(It.IsAny<Guid>()))
                            .ReturnsAsync(() => new MemoryStream(TestData.JpegImage));
                        return fileService.Object;
                    }
                });
            });

    [MemberNotNull(nameof(_playwright), nameof(_browser), nameof(_host))]
    private void ThrowIfNotInitialized()
    {
        if (!_initialized || _playwright is null || _browser is null || _host is null)
        {
            throw new InvalidOperationException("Fixture has not been initialized");
        }
    }

    private async Task AddTestAppToApplicationUsers()
    {
        await using var dbContext = await Services.GetRequiredService<IDbContextFactory<TrsDbContext>>().CreateDbContextAsync();

        dbContext.ApplicationUsers.Add(new Core.DataStore.Postgres.Models.ApplicationUser()
        {
            UserId = Guid.NewGuid(),
            Name = "Test App",
            IsOidcClient = true,
            ClientId = TestAppConfiguration.ClientId,
            ClientSecret = TestAppConfiguration.ClientSecret,
            RedirectUris = [BaseUrl + TestAppConfiguration.RedirectUriPath],
            PostLogoutRedirectUris = [BaseUrl + TestAppConfiguration.PostLogoutRedirectUriPath],
            OneLoginAuthenticationSchemeName = FakeOneLoginAuthenticationScheme
        });

        await dbContext.SaveChangesAsync();
    }

    async Task IStartupTask.Execute()
    {
        _host = CreateHost();

        _playwright = await Playwright.CreateAsync();

        var browserOptions = new BrowserTypeLaunchOptions()
        {
            Timeout = 10000,
            Args = new[] { "--start-maximized" }
        };

        if (Debugger.IsAttached)
        {
            browserOptions.Headless = false;
            browserOptions.Devtools = true;
            browserOptions.SlowMo = 250;
        }

        _browser = await _playwright.Chromium.LaunchAsync(browserOptions);

        _initialized = true;

        await AddTestAppToApplicationUsers();
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (Browser != null)
        {
            await Browser.DisposeAsync();
        }

        _playwright?.Dispose();

        if (_host != null)
        {
            await _host.DisposeAsync();
        }
    }

    public sealed class Host<T> : IAsyncDisposable
        where T : class
    {
        private readonly KestrelWebApplicationFactory<T> _applicationFactory;

        private Host(KestrelWebApplicationFactory<T> applicationFactory)
        {
            _applicationFactory = applicationFactory;
        }

        public IServiceProvider Services => _applicationFactory.Services;

        public static Host<T> CreateHost(
            string url,
            Action<IWebHostBuilder> configureWebHostBuilder)
        {
            var applicationFactory = new KestrelWebApplicationFactory<T>(url, configureWebHostBuilder);
            _ = applicationFactory.Services;  // Starts the server
            return new Host<T>(applicationFactory);
        }

        public ValueTask DisposeAsync() => _applicationFactory.DisposeAsync();

        // See https://github.com/dotnet/aspnetcore/issues/4892
        private class KestrelWebApplicationFactory<TFactory> : WebApplicationFactory<TFactory>
            where TFactory : class
        {
            private readonly Action<IWebHostBuilder> _configureWebHostBuilder;
            private IHost? _host;

            public KestrelWebApplicationFactory(string url, Action<IWebHostBuilder> configureWebHostBuilder)
            {
                Url = url;
                _configureWebHostBuilder = configureWebHostBuilder;
            }

            public override IServiceProvider Services
            {
                get
                {
                    EnsureServer();
                    return _host!.Services!;
                }
            }

            public string Url { get; }

            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder
                    .UseUrls(Url)
                    .UseEnvironment("EndToEndTests");

                _configureWebHostBuilder(builder);
            }

            protected override IHost CreateHost(IHostBuilder builder)
            {
                // We need to return a host configured with TestServer, even though we're not going to use it.
                // Configure an empty dummy web app with TestServer.
                var dummyBuilder = new HostBuilder();
                dummyBuilder.ConfigureWebHost(webBuilder => webBuilder.Configure(app => { }).UseTestServer());
                var testHost = dummyBuilder.Build();
                testHost.Start();

                builder.ConfigureWebHost(webHostBuilder => webHostBuilder.UseKestrel());

                _host = builder.Build();
                _host.Start();

                return testHost;
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    _host?.Dispose();
                }
            }

            private void EnsureServer()
            {
                if (_host is null)
                {
                    // This forces WebApplicationFactory to bootstrap the server
                    using var _ = CreateDefaultClient();
                }
            }
        }
    }
}

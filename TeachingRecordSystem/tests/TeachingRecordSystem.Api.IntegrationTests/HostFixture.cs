using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using JustEat.HttpClientInterception;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.IdentityModel.Tokens;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.IntegrationTests;
using TeachingRecordSystem.Api.IntegrationTests.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.Core.Services.Webhooks;

[assembly: AssemblyFixture(typeof(HostFixture))]

namespace TeachingRecordSystem.Api.IntegrationTests;

public class HostFixture : InitializeDbFixture
{
    private readonly ApiWebApplicationFactory _webApplicationFactory;

    public HostFixture()
    {
        using (var rsa = RSA.Create())
        {
            JwtSigningCredentials = new SigningCredentials(
                new RsaSecurityKey(rsa.ExportParameters(includePrivateParameters: true)),
                SecurityAlgorithms.RsaSha256);
        }

        _webApplicationFactory = new ApiWebApplicationFactory(this);
    }

    public static Guid DefaultApplicationUserId { get; } = new("c0c8c511-e8e4-4b8e-96e3-55085dafc05d");

    public static Guid GetAnIdentityApplicationUserId { get; } = new("873f0cb0-7174-4256-921a-e8a8aaa06361");

    public SigningCredentials JwtSigningCredentials { get; }

    public IServiceProvider Services => _webApplicationFactory.Services;

    public void ConfigureEvidenceFilesHttpClient(Action<HttpClientInterceptorOptions> configure) =>
        configure(TestScopedServices.GetCurrent().EvidenceFilesHttpClientInterceptorOptions);

    public HttpClient CreateClient() => _webApplicationFactory.CreateClient();

    public HttpClient CreateClient(WebApplicationFactoryClientOptions options) => _webApplicationFactory.CreateClient(options);

    public static void AddApplicationUsers(TrsDbContext dbContext)
    {
        dbContext.ApplicationUsers.Add(new Core.DataStore.Postgres.Models.ApplicationUser()
        {
            UserId = DefaultApplicationUserId,
            Name = "Tests",
            ApiRoles = ApiRoles.All.ToArray()
        });

        dbContext.ApplicationUsers.Add(new Core.DataStore.Postgres.Models.ApplicationUser()
        {
            UserId = GetAnIdentityApplicationUserId,
            Name = "Get an identity",
            ApiRoles = [ApiRoles.UpdatePerson]
        });

        dbContext.SaveChanges();
    }

    public override async ValueTask InitializeAsync()
    {
        await InitializeDbAsync();

        _ = Services;  // Start the server
    }

    private class ApiWebApplicationFactory(HostFixture hostFixture) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Tests");

            // N.B. Don't use builder.ConfigureAppConfiguration here since it runs *after* the entry point
            // i.e. Program.cs and that has a dependency on IConfiguration
            var configuration = TestConfiguration.GetConfiguration()
                .AddInMemoryCollection([
                    KeyValuePair.Create("GetAnIdentityApplicationUserId", (string?)GetAnIdentityApplicationUserId.ToString())
                ])
                .Build();
            builder.UseConfiguration(configuration);

            builder.ConfigureServices((context, services) =>
            {
                // Replace authentication handlers with mechanisms we can control from tests
                services.Configure<AuthenticationOptions>(options =>
                {
                    options.SchemeMap[ApiKeyAuthenticationHandler.AuthenticationScheme].HandlerType = typeof(TestApiKeyAuthenticationHandler);
                    options.SchemeMap["IdAccessToken"].HandlerType = typeof(SimpleJwtBearerAuthentication);
                    options.SchemeMap["AuthorizeAccessAccessToken"].HandlerType = typeof(SimpleJwtBearerAuthentication);
                });

                services.Configure<SimpleJwtBearerAuthenticationOptions>("IdAccessToken", o => o.IssuerSigningKey = hostFixture.JwtSigningCredentials.Key);
                services.Configure<SimpleJwtBearerAuthenticationOptions>("AuthorizeAccessAccessToken", o => o.IssuerSigningKey = hostFixture.JwtSigningCredentials.Key);

                // Add controllers defined in this test assembly
                services.AddMvc().AddApplicationPart(typeof(HostFixture).Assembly);

                services
                    .AddSingleton(DbHelper.Instance)
                    .AddSingleton<TestData>()
                    .AddSingleton<CurrentApiClientProvider>()
                    .AddSingleton<INotificationSender, NoopNotificationSender>()
                    .AddSingleton<IStartupFilter, ExecuteScheduledJobsStartupFilter>();

                services.Configure<GetAnIdentityOptions>(options =>
                {
                    options.TokenEndpoint = "dummy";
                    options.ClientId = "dummy";
                    options.ClientSecret = "dummy";
                    options.BaseAddress = "dummy";
                });

                services.Configure<WebhookOptions>(options =>
                {
                    using var key = ECDsa.Create(ECCurve.NamedCurves.nistP384);
                    var certRequest = new CertificateRequest("CN=Teaching Record System Tests", key, HashAlgorithmName.SHA384);
                    using var cert = certRequest.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddDays(1));
                    var certPem = cert.ExportCertificatePem();
                    var keyPem = key.ExportECPrivateKeyPem();

                    options.CanonicalDomain = "http://localhost";
                    options.SigningKeyId = "testkey";
                    options.Keys = [
                        new WebhookOptionsKey()
                        {
                            KeyId = "testkey",
                            CertificatePem = certPem,
                            PrivateKeyPem = keyPem,
                        }];
                });

                services.AddHttpClient("EvidenceFiles")
                    .ConfigurePrimaryHttpMessageHandler(_ => new DelegateToEvidenceFilesHandler());

                TestScopedServices.ConfigureServices(services);
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            // Ensure we can flow AsyncLocals from tests to the server
            builder.ConfigureServices(services => services.Configure<TestServerOptions>(o => o.PreserveExecutionContext = true));

            return base.CreateHost(builder);
        }
    }

    // HttpClient caches these handlers so we can't use TestScopedServices.GetCurrent().EvidenceFilesHttpClientInterceptorOptions.CreateHttpMessageHandler()
    // since it will persist for multiple tests.
    // This wrapper type delegates to the current test-scoped instance.
    private class DelegateToEvidenceFilesHandler : DelegatingHandler
    {
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var testScopedHandler = TestScopedServices.GetCurrent().EvidenceFilesHttpClientInterceptorOptions.CreateHttpMessageHandler();
            var result = typeof(DelegatingHandler).GetMethod(nameof(SendAsync), BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(testScopedHandler, [request, cancellationToken]);
            return (Task<HttpResponseMessage>)result!;
        }
    }

    private class ExecuteScheduledJobsStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
            app =>
            {
                app.Use(async (_, next) =>
                {
                    await next();

                    await TestScopedServices.GetCurrent().BackgroundJobScheduler.ExecuteDeferredJobsAsync();
                });

                next(app);
            };
    }
}

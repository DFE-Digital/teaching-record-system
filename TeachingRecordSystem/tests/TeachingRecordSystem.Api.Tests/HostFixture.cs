using System.Security.Cryptography;
using JustEat.HttpClientInterception;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.Tests.Infrastructure.Security;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.Certificates;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.TrnGenerationApi;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Api.Tests;

public class HostFixture : WebApplicationFactory<Program>
{
    private readonly IConfiguration _configuration;

    public HostFixture(IConfiguration configuration)
    {
        _configuration = configuration;

        using (var rsa = RSA.Create())
        {
            JwtSigningCredentials = new SigningCredentials(new RsaSecurityKey(rsa.ExportParameters(includePrivateParameters: true)), SecurityAlgorithms.RsaSha256);
        }

        _ = Services;  // Start the host
    }

    public HttpClientInterceptorOptions EvidenceFilesHttpClientInterceptorOptions { get; } = new();

    public SigningCredentials JwtSigningCredentials { get; }

    public void ConfigureEvidenceFilesHttpClient(Action<HttpClientInterceptorOptions> configure) =>
        configure(EvidenceFilesHttpClientInterceptorOptions);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // N.B. Don't use builder.ConfigureAppConfiguration here since it runs *after* the entry point
        // i.e. Program.cs and that has a dependency on IConfiguration
        builder.UseConfiguration(_configuration);

        builder.ConfigureServices((context, services) =>
        {
            DbHelper.ConfigureDbServices(services, context.Configuration.GetRequiredConnectionString("DefaultConnection"));

            // Replace authentication handlers with mechanisms we can control from tests
            services.Configure<AuthenticationOptions>(options =>
            {
                options.SchemeMap[ApiKeyAuthenticationHandler.AuthenticationScheme].HandlerType = typeof(TestApiKeyAuthenticationHandler);
                options.SchemeMap["IdAccessToken"].HandlerType = typeof(SimpleJwtBearerAuthentication);
                options.SchemeMap["AuthorizeAccessAccessToken"].HandlerType = typeof(SimpleJwtBearerAuthentication);
            });

            services.Configure<SimpleJwtBearerAuthenticationOptions>("IdAccessToken", o => o.IssuerSigningKey = JwtSigningCredentials.Key);
            services.Configure<SimpleJwtBearerAuthenticationOptions>("AuthorizeAccessAccessToken", o => o.IssuerSigningKey = JwtSigningCredentials.Key);

            // Add controllers defined in this test assembly
            services.AddMvc().AddApplicationPart(typeof(HostFixture).Assembly);
            services.AddSingleton<CurrentApiClientProvider>();
            services.AddTestScoped<IClock>(tss => tss.Clock);
            services.AddTestScoped<IDataverseAdapter>(tss => tss.DataverseAdapterMock.Object);
            services.AddTestScoped<IGetAnIdentityApiClient>(tss => tss.GetAnIdentityApiClientMock.Object);
            services.AddTestScoped<IOptions<AccessYourTeachingQualificationsOptions>>(tss => tss.AccessYourTeachingQualificationsOptions);
            services.AddTestScoped<ICertificateGenerator>(tss => tss.CertificateGeneratorMock.Object);
            services.AddSingleton<TestData>(
                sp => ActivatorUtilities.CreateInstance<TestData>(
                    sp,
                    (IClock)new ForwardToTestScopedClock(),
                    TestDataSyncConfiguration.Sync(sp.GetRequiredService<TrsDataSyncHelper>())));
            services.AddFakeXrm();
            services.AddSingleton<FakeTrnGenerator>();
            services.AddSingleton<TrsDataSyncHelper>();
            services.AddSingleton<ITrnGenerationApiClient, FakeTrnGenerationApiClient>();
            services.Decorate<ICrmQueryDispatcher>(
                inner => new CrmQueryDispatcherDecorator(
                    inner,
                    TestScopedServices.TryGetCurrent(out var tss) ? tss.CrmQueryDispatcherSpy : new()));

            services.Configure<GetAnIdentityOptions>(options =>
            {
                options.TokenEndpoint = "dummy";
                options.ClientId = "dummy";
                options.ClientSecret = "dummy";
                options.BaseAddress = "dummy";
                options.WebHookClientSecret = "dummy";
            });

            services.AddHttpClient("EvidenceFiles")
                .AddHttpMessageHandler(_ => EvidenceFilesHttpClientInterceptorOptions.CreateHttpMessageHandler())
                .ConfigurePrimaryHttpMessageHandler(_ => new NotFoundHandler());
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Ensure we can flow AsyncLocals from tests to the server
        builder.ConfigureServices(services => services.Configure<TestServerOptions>(o => o.PreserveExecutionContext = true));

        return base.CreateHost(builder);
    }

    private class NotFoundHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
        }
    }

    private class ForwardToTestScopedClock : IClock
    {
        public DateTime UtcNow => TestScopedServices.GetCurrent().Clock.UtcNow;
    }
}

file static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTestScoped<T>(this IServiceCollection services, Func<TestScopedServices, T> resolveService)
        where T : class
    {
        return services.AddTransient<T>(_ => resolveService(TestScopedServices.GetCurrent()));
    }
}

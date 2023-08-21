using System.Security.Cryptography;
using JustEat.HttpClientInterception;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.AccessYourQualifications;
using TeachingRecordSystem.Core.Services.Certificates;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;

namespace TeachingRecordSystem.Api.Tests;

public class ApiFixture : WebApplicationFactory<Program>
{
    private readonly IConfiguration _configuration;

    public ApiFixture(IConfiguration configuration)
    {
        _configuration = configuration;
        JwtSigningCredentials = new SigningCredentials(new RsaSecurityKey(RSA.Create()), SecurityAlgorithms.RsaSha256);
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

            // Add controllers defined in this test assembly
            services.AddMvc().AddApplicationPart(typeof(ApiFixture).Assembly);

            services.AddTestScoped<IClock>(tss => tss.Clock);
            services.AddTestScoped<IDataverseAdapter>(tss => tss.DataverseAdapterMock.Object);
            services.AddTestScoped<IGetAnIdentityApiClient>(tss => tss.GetAnIdentityApiClientMock.Object);
            services.AddTestScoped<IOptions<AccessYourQualificationsOptions>>(tss => tss.AccessYourQualificationsOptions);
            services.AddTestScoped<IOptions<GetAnIdentityOptions>>(tss => tss.GetAnIdentityOptions);
            services.AddTestScoped<ICertificateGenerator>(tss => tss.CertificateGeneratorMock.Object);
            services.AddSingleton<IOrganizationServiceAsync, FakeServiceClient>();
            services.AddSingleton<TestData>();
            services.AddSingleton<SeedReferenceData>();
            services.AddStartupTask<SeedReferenceData>();

            services.AddHttpClient("EvidenceFiles")
                .AddHttpMessageHandler(_ => EvidenceFilesHttpClientInterceptorOptions.CreateHttpMessageHandler())
                .ConfigurePrimaryHttpMessageHandler(_ => new NotFoundHandler());

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters.ValidateIssuer = false;
                options.TokenValidationParameters.IssuerSigningKey = JwtSigningCredentials.Key;
            });
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
}

file static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTestScoped<T>(this IServiceCollection services, Func<TestScopedServices, T> resolveService)
        where T : class
    {
        return services.AddTransient<T>(_ => resolveService(TestScopedServices.GetCurrent()));
    }
}

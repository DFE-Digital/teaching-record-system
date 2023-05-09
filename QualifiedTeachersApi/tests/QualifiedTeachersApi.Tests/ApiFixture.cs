using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using JustEat.HttpClientInterception;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.Services.Certificates;
using QualifiedTeachersApi.Services.GetAnIdentityApi;
using QualifiedTeachersApi.Tests.Infrastructure;

namespace QualifiedTeachersApi.Tests;

public class ApiFixture : WebApplicationFactory<Program>
{
    private readonly HttpClientInterceptorOptions _evidenceFilesHttpClientInterceptorOptions = new();
    private readonly TestConfiguration _testConfiguration;

    public ApiFixture(TestConfiguration testConfiguration, DbHelper dbHelper)
    {
        _testConfiguration = testConfiguration;
        DbHelper = dbHelper;
        JwtSigningCredentials = new SigningCredentials(new RsaSecurityKey(RSA.Create()), SecurityAlgorithms.RsaSha256);
    }

    public DbHelper DbHelper { get; }

    public Mock<IDataverseAdapter> DataverseAdapter { get; } = new Mock<IDataverseAdapter>();

    public Mock<IGetAnIdentityApiClient> IdentityApiClient { get; } = new Mock<IGetAnIdentityApiClient>();

    public Mock<IOptions<GetAnIdentityOptions>> GetAnIdentityOptions { get; } = new Mock<IOptions<GetAnIdentityOptions>>();

    public Mock<ICertificateGenerator> CertificateGenerator { get; } = new Mock<ICertificateGenerator>();

    public SigningCredentials JwtSigningCredentials { get; }

    public void ConfigureEvidenceFilesHttpClient(Action<HttpClientInterceptorOptions> configure) =>
        configure(_evidenceFilesHttpClientInterceptorOptions);

    public async Task Initialize()
    {
        await DbHelper.EnsureSchema();
    }

    public void ResetMocks()
    {
        _evidenceFilesHttpClientInterceptorOptions.Clear();
        DataverseAdapter.Reset();
        GetAnIdentityOptions.Reset();
        IdentityApiClient.Reset();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // N.B. Don't use builder.ConfigureAppConfiguration here since it runs *after* the entry point
        // i.e. Program.cs and that has a dependency on IConfiguration
        builder.UseConfiguration(_testConfiguration.Configuration);

        builder.ConfigureServices(services =>
        {
            // Add controllers defined in this test assembly
            services.AddMvc().AddApplicationPart(typeof(ApiFixture).Assembly);

            services.AddSingleton(DataverseAdapter.Object);
            services.AddSingleton(IdentityApiClient.Object);
            services.AddSingleton(GetAnIdentityOptions.Object);
            services.AddSingleton(CertificateGenerator.Object);
            services.AddSingleton<IClock, TestableClock>();

            services.AddHttpClient("EvidenceFiles")
                .AddHttpMessageHandler(_ => _evidenceFilesHttpClientInterceptorOptions.CreateHttpMessageHandler())
                .ConfigurePrimaryHttpMessageHandler(_ => new NotFoundHandler());

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters.ValidateIssuer = false;
                options.TokenValidationParameters.IssuerSigningKey = JwtSigningCredentials.Key;
            });
        });
    }

    private class NotFoundHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
        }
    }
}

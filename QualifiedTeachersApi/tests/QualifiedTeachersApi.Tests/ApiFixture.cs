#nullable disable
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using JustEat.HttpClientInterception;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.Services.Certificates;
using QualifiedTeachersApi.Services.GetAnIdentityApi;
using Xunit;

namespace QualifiedTeachersApi.Tests;

public class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly HttpClientInterceptorOptions _evidenceFilesHttpClientInterceptorOptions = new();

    public ApiFixture()
    {
        JwtSigningCredentials = new SigningCredentials(new RsaSecurityKey(RSA.Create()), SecurityAlgorithms.RsaSha256);
    }

    public DbHelper DbHelper => Services.GetRequiredService<DbHelper>();

    public Mock<IDataverseAdapter> DataverseAdapter { get; } = new Mock<IDataverseAdapter>();

    public Mock<IGetAnIdentityApiClient> IdentityApiClient { get; } = new Mock<IGetAnIdentityApiClient>();

    public Mock<IOptions<GetAnIdentityOptions>> GetAnIdentityOptions { get; } = new Mock<IOptions<GetAnIdentityOptions>>();

    public Mock<ICertificateGenerator> CertificateGenerator { get; } = new Mock<ICertificateGenerator>();

    public SigningCredentials JwtSigningCredentials { get; }

    public void ConfigureEvidenceFilesHttpClient(Action<HttpClientInterceptorOptions> configure) =>
        configure(_evidenceFilesHttpClientInterceptorOptions);

    public async Task InitializeAsync()
    {
        await DbHelper.ResetSchema();
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
        builder.UseConfiguration(GetTestConfiguration());

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

            services.AddSingleton(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                return new DbHelper(connectionString);
            });

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters.ValidateIssuer = false;
                options.TokenValidationParameters.IssuerSigningKey = JwtSigningCredentials.Key;
            });
        });
    }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    private static IConfiguration GetTestConfiguration() =>
        new ConfigurationBuilder().AddUserSecrets<ApiFixture>(optional: true).Build();

    private class NotFoundHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
        }
    }
}

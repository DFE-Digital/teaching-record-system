using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace DqtApi.FunctionalTests
{
    public sealed class ApiFixture : IDisposable
    {
        public ApiFixture()
        {
            Configuration = GetConfiguration();
            HttpClient = CreateHttpClient();
        }

        public IConfiguration Configuration { get; }

        public HttpClient HttpClient { get; }

        public void Dispose()
        {
            HttpClient.Dispose();
        }

        private HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(Configuration["BaseUrl"])
            };
            return httpClient;
        }

        private static IConfiguration GetConfiguration()
        {
            var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (environmentName.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                configurationBuilder.AddUserSecrets(typeof(ApiFixture).Assembly);
            }

            return configurationBuilder.Build();
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace DqtApi.FunctionalTests
{
    public sealed class ApiFixture : IDisposable
    {
        private Process _apiProcess;

        public ApiFixture()
        {
            EnvironmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

            Configuration = GetConfiguration();
            HttpClient = CreateHttpClient();
            StartLocalApi();
        }

        public IConfiguration Configuration { get; }

        public string EnvironmentName { get; }

        public HttpClient HttpClient { get; }

        public void Dispose()
        {
            HttpClient.Dispose();
            StopLocalApi();
        }

        private HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(Configuration["BaseUrl"])
            };
            return httpClient;
        }

        private IConfiguration GetConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (EnvironmentName.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                configurationBuilder.AddUserSecrets(typeof(ApiFixture).Assembly);
            }

            return configurationBuilder.Build();
        }

        private void StartLocalApi()
        {
            var port = new Uri(Configuration["BaseUrl"]).Port;

            if (EnvironmentName.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                // Start the API project locally to run tests against if it's not already running

                if (IsServerListening())
                {
                    return;
                }

                var apiProjectLocation = GetApiHostProjectFolder();

                _apiProcess = new Process();
                _apiProcess.StartInfo.FileName = "dotnet";
                _apiProcess.StartInfo.Arguments = "run";
                _apiProcess.StartInfo.WorkingDirectory = apiProjectLocation;
                _apiProcess.StartInfo.CreateNoWindow = true;
                _apiProcess.StartInfo.UseShellExecute = false;
                _apiProcess.StartInfo.Environment.Add("ASPNETCORE_ENVIRONMENT", "Development");

                _apiProcess.Start();

                WaitForServerToStart();
            }

            static string GetApiHostProjectFolder()
            {
                var path = Path.Combine(
                    Assembly.GetExecutingAssembly().Location,
                    "..",
                    "..",
                    "..",
                    "..",
                    "..",
                    "..",
                    "src",
                    "DqtApi");

                return new DirectoryInfo(path).FullName;
            }

            bool IsServerListening()
            {
                var ipEndpoints = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
                return ipEndpoints.Any(e => e.Port == port);
            }

            void WaitForServerToStart()
            {
                const int maxWaitTimeMs = 5000;
                var waitStopwatch = Stopwatch.StartNew();

                while (waitStopwatch.ElapsedMilliseconds <= maxWaitTimeMs)
                {
                    if (IsServerListening())
                    {
                        return;
                    }

                    Thread.Sleep(100);
                }

                throw new TimeoutException($"Timeout elapsed waiting for API to be available on port {port}.");
            }
        }

        private void StopLocalApi()
        {
            if (_apiProcess != null)
            {
                // TODO Use new signal APIs from .NET 6 when available for more graceful shutdown
                // https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.posixsignalregistration.create?view=net-6.0

                _apiProcess.Kill();
                _apiProcess.WaitForExit();
                _apiProcess.Dispose();
            }
        }
    }
}

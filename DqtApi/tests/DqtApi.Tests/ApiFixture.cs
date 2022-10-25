using System.Threading.Tasks;
using DqtApi.DataStore.Crm;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace DqtApi.Tests
{
    public class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public DbHelper DbHelper => Services.GetRequiredService<DbHelper>();

        public Mock<IDataverseAdapter> DataverseAdapter { get; } = new Mock<IDataverseAdapter>();

        public async Task InitializeAsync()
        {
            await DbHelper.ResetSchema();
        }

        public void ResetMocks()
        {
            DataverseAdapter.Reset();
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
                services.AddSingleton<IClock, TestableClock>();

                services.AddSingleton(sp =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    return new DbHelper(connectionString);
                });
            });
        }

        Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

        private static IConfiguration GetTestConfiguration() =>
            new ConfigurationBuilder().AddUserSecrets<ApiFixture>(optional: true).Build();
    }
}

using System.Threading.Tasks;
using DqtApi.DAL;
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

        public Mock<IDataverseAdaptor> DataverseAdaptor { get; } = new Mock<IDataverseAdaptor>();

        public async Task InitializeAsync()
        {
            await DbHelper.ResetSchema();
        }

        public void ResetMocks()
        {
            DataverseAdaptor.Reset();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration(config => config.AddUserSecrets<ApiFixture>(optional: true));

            builder.ConfigureServices(services =>
            {
                // Add controllers defined in this test assembly
                services.AddMvc().AddApplicationPart(typeof(ApiFixture).Assembly);

                services.AddSingleton(DataverseAdaptor.Object);

                services.AddSingleton(sp =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    return new DbHelper(connectionString);
                });
            });
        }

        Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
    }
}

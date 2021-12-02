using System.Threading.Tasks;
using DqtApi.DAL;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;
using Moq;
using Xunit;

namespace DqtApi.Tests
{
    public class ApiFixture : WebApplicationFactory<DqtApi.Program>, IAsyncLifetime
    {
        public DbHelper DbHelper => Services.GetRequiredService<DbHelper>();

        public Mock<IOrganizationServiceAsync> OrganizationService { get; } = new Mock<IOrganizationServiceAsync>();

        public async Task InitializeAsync()
        {
            await DbHelper.ResetSchema();
        }

        public void ResetMocks()
        {
            OrganizationService.Reset();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration(config => config.AddUserSecrets<ApiFixture>(optional: true));

            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IOrganizationServiceAsync>(OrganizationService.Object);
                services.AddSingleton<IDataverseAdaptor, DataverseAdaptor>();

                services.AddSingleton<DbHelper>(sp =>
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

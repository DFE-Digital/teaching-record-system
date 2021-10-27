using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;
using Moq;

namespace DqtApi.Tests
{
    public class ApiFixture : WebApplicationFactory<DqtApi.Startup>
    {
        public Mock<IOrganizationServiceAsync> OrganizationService { get; } = new Mock<IOrganizationServiceAsync>();

        public void ResetMocks()
        {
            OrganizationService.Reset();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IOrganizationServiceAsync>(OrganizationService.Object);
            });
        }
    }
}
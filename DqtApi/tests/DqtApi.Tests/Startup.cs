using DqtApi.Tests.DataverseIntegration;
using Microsoft.Extensions.DependencyInjection;

namespace DqtApi.Tests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<CrmClientFixture>();
            services.AddMemoryCache();
        }
    }
}

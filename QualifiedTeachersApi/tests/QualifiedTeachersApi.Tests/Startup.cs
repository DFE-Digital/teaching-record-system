using Microsoft.Extensions.DependencyInjection;
using QualifiedTeachersApi.Tests.DataverseIntegration;

namespace QualifiedTeachersApi.Tests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<CrmClientFixture>();
        services.AddMemoryCache();
    }
}

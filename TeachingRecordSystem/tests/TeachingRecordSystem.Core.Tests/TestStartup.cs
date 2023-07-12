using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Tests;

public class TestStartup : ITestStartup
{
    public void ConfigureConfiguration(IConfigurationBuilder builder)
    {
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
    }
}

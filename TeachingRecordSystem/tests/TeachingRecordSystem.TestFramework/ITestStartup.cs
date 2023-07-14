using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.TestFramework;

public interface ITestStartup
{
    void ConfigureConfiguration(IConfigurationBuilder builder);
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    public Task Initialize(IServiceProvider services) => Task.CompletedTask;
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.TestCommon;

public class ServiceProviderFixture : InitializeDbFixture
{
    public ServiceProviderFixture()
    {
        var configuration = TestConfiguration.GetConfiguration();

        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton(DbHelper.Instance)
            .AddDatabase(configuration.GetPostgresConnectionString());

        // ReSharper disable once VirtualMemberCallInConstructor
        ConfigureServices(services, configuration);
        Services = services.BuildServiceProvider();
    }

    public IServiceProvider Services { get; set; }

    protected virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
    }
}

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

    public async Task WithServiceAsync<TService>(Func<TService, Task> action, params object[] arguments)
        where TService : notnull
    {
        using var scope = Services.CreateScope();
        var service = ActivatorUtilities.CreateInstance<TService>(scope.ServiceProvider, arguments);
        await action(service);
    }

    public async Task<TResult> WithServiceAsync<TService, TResult>(Func<TService, Task<TResult>> action, params object[] arguments)
        where TService : notnull
    {
        using var scope = Services.CreateScope();
        var service = ActivatorUtilities.CreateInstance<TService>(scope.ServiceProvider, arguments);
        return await action(service);
    }

    protected virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
    }
}

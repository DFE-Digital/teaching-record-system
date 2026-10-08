using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.TestCommon.Database;
using Xunit;

namespace TeachingRecordSystem.TestCommon;

public class ServiceProviderFixture : IAsyncLifetime
{
    public ServiceProviderFixture()
    {
        var configuration = TestConfiguration.GetConfiguration();

        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddDatabase(configuration.GetPostgresConnectionString());

        // ReSharper disable once VirtualMemberCallInConstructor
        ConfigureServices(services, configuration);

        // Registered last so it wins: every TrsDbContext is built against the database leased by the
        // running test.
        services.AddPooledTestDatabase();

        Services = services.BuildServiceProvider();
    }

    public IServiceProvider Services { get; set; }

    public virtual async ValueTask InitializeAsync() => await TestDatabases.InitializeAsync();

    public virtual async ValueTask DisposeAsync() => await TestDatabases.DisposeAsync();

    public void WithService<TService>(Action<TService> action, params object[] arguments)
        where TService : notnull
    {
        using var scope = Services.CreateScope();
        var service = ActivatorUtilities.CreateInstance<TService>(scope.ServiceProvider, arguments);
        action(service);
    }

    public TResult WithService<TService, TResult>(Func<TService, TResult> action, params object[] arguments)
        where TService : notnull
    {
        using var scope = Services.CreateScope();
        var service = ActivatorUtilities.CreateInstance<TService>(scope.ServiceProvider, arguments);
        return action(service);
    }

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

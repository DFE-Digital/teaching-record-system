using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.TestCommon.Database;

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

        // Registered last so it wins: every TrsDbContext is built against the database leased by the
        // running test rather than one shared database.
        if (UsePooledDatabase)
        {
            services.AddPooledTestDatabase();
        }

        Services = services.BuildServiceProvider();
    }

    public IServiceProvider Services { get; set; }

    // Opt in to give every test its own database. Fixtures that haven't migrated keep using the single
    // shared database that DbHelper manages.
    protected virtual bool UsePooledDatabase => false;

    public override async ValueTask InitializeAsync()
    {
        if (UsePooledDatabase)
        {
            await TestDatabases.InitializeAsync();
            return;
        }

        await base.InitializeAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        if (UsePooledDatabase)
        {
            await TestDatabases.DisposeAsync();
        }

        await base.DisposeAsync();
    }

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

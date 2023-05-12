using Microsoft.EntityFrameworkCore;
using QualifiedTeachersApi.DataStore.Sql;
using QualifiedTeachersApi.Tests.Infrastructure;
using Xunit;

namespace QualifiedTeachersApi.Tests;

public class DbFixture : IAsyncLifetime
{
    public DbFixture(TestConfiguration testConfiguration, DbHelper dbHelper)
    {
        Configuration = testConfiguration.Configuration;
        ConnectionString = Configuration.GetConnectionString("DefaultConnection") ??
            throw new Exception("Connection string DefaultConnection is missing.");
        DbHelper = dbHelper;
        Services = GetServices();
    }

    public IConfiguration Configuration { get; }

    public string ConnectionString { get; }

    public DbHelper DbHelper { get; }

    public IServiceProvider Services { get; }

    public DqtContext GetDbContext() => Services.GetRequiredService<DqtContext>();

    public IDbContextFactory<DqtContext> GetDbContextFactory() => Services.GetRequiredService<IDbContextFactory<DqtContext>>();

    public async Task InitializeAsync()
    {
        await DbHelper.EnsureSchema();
    }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    private IServiceProvider GetServices()
    {
        var services = new ServiceCollection();

        services.AddDbContext<DqtContext>(
            options => DqtContext.ConfigureOptions(options, ConnectionString),
            contextLifetime: ServiceLifetime.Transient);

        services.AddDbContextFactory<DqtContext>(
            options => DqtContext.ConfigureOptions(options, ConnectionString));

        return services.BuildServiceProvider();
    }
}

using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Api.Tests.Infrastructure;
using Xunit;

namespace TeachingRecordSystem.Api.Tests;

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

    public TrsDbContext GetDbContext() => Services.GetRequiredService<TrsDbContext>();

    public IDbContextFactory<TrsDbContext> GetDbContextFactory() => Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    public async Task InitializeAsync()
    {
        await DbHelper.EnsureSchema();
    }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    private IServiceProvider GetServices()
    {
        var services = new ServiceCollection();

        services.AddDbContext<TrsDbContext>(
            options => TrsDbContext.ConfigureOptions(options, ConnectionString),
            contextLifetime: ServiceLifetime.Transient);

        services.AddDbContextFactory<TrsDbContext>(
            options => TrsDbContext.ConfigureOptions(options, ConnectionString));

        return services.BuildServiceProvider();
    }
}

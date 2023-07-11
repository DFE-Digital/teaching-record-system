using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Api.Tests;

public class DbFixture : IAsyncLifetime
{
    public DbFixture(IConfiguration configuration, DbHelper dbHelper)
    {
        Configuration = configuration;
        ConnectionString = dbHelper.ConnectionString;
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

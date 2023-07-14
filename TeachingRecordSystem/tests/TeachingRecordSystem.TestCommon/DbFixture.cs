using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.TestCommon;

public class DbFixture
{
    public DbFixture(DbHelper dbHelper, IServiceProvider serviceProvider)
    {
        DbHelper = dbHelper;
        Services = serviceProvider;
    }

    public DbHelper DbHelper { get; }

    public IServiceProvider Services { get; }

    public TrsDbContext GetDbContext() => Services.GetRequiredService<TrsDbContext>();

    public IDbContextFactory<TrsDbContext> GetDbContextFactory() => Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();
}

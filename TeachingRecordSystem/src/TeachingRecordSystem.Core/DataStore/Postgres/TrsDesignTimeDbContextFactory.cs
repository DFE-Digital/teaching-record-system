using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TeachingRecordSystem.Core.DataStore.Postgres;

[UsedImplicitly]
public class TrsDesignTimeDbContextFactory : IDesignTimeDbContextFactory<TrsDbContext>
{
    public TrsDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<TrsDesignTimeDbContextFactory>(optional: true)  // Optional for CI
            .Build();

        var connectionString = configuration.GetConnectionString(TrsDbContext.ConnectionName);

        return TrsDbContext.Create(connectionString);
    }
}

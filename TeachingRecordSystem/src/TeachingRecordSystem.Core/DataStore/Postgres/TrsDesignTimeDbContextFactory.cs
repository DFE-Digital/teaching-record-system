using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TeachingRecordSystem.Core.DataStore.Postgres;

public class TrsDesignTimeDbContextFactory : IDesignTimeDbContextFactory<TrsDbContext>
{
    public TrsDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<TrsDesignTimeDbContextFactory>(optional: true)  // Optional for CI
            .Build();

        var connectionString = configuration.GetPostgresConnectionString();

        return TrsDbContext.Create(connectionString);
    }
}

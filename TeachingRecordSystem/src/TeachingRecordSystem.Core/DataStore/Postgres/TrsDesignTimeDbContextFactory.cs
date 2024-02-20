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

        var connectionString = configuration.GetRequiredConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<TrsDbContext>();
        TrsDbContext.ConfigureOptions(optionsBuilder, connectionString);

        return new TrsDbContext(optionsBuilder.Options);
    }
}

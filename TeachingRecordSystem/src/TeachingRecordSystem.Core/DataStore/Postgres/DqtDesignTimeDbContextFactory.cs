using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TeachingRecordSystem.Core.DataStore.Postgres;

public class TrsDesignTimeDbContextFactory : IDesignTimeDbContextFactory<TrsContext>
{
    public TrsContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<TrsDesignTimeDbContextFactory>(optional: true)  // Optional for CI
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection") ??
            throw new Exception("Connection string DefaultConnection is missing.");

        var optionsBuilder = new DbContextOptionsBuilder<TrsContext>();
        TrsContext.ConfigureOptions(optionsBuilder, connectionString);

        return new TrsContext(optionsBuilder.Options);
    }
}

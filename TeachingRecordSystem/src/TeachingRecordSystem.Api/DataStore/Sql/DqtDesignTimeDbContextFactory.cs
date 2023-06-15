using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TeachingRecordSystem.Api.DataStore.Sql;

public class DqtDesignTimeDbContextFactory : IDesignTimeDbContextFactory<DqtContext>
{
    public DqtContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<DqtDesignTimeDbContextFactory>(optional: true)  // Optional for CI
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection") ??
            throw new Exception("Connection string DefaultConnection is missing.");

        var optionsBuilder = new DbContextOptionsBuilder<DqtContext>();
        DqtContext.ConfigureOptions(optionsBuilder, connectionString);

        return new DqtContext(optionsBuilder.Options);
    }
}

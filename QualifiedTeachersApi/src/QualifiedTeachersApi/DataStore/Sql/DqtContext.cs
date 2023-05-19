using Microsoft.EntityFrameworkCore;
using QualifiedTeachersApi.DataStore.Sql.Models;

namespace QualifiedTeachersApi.DataStore.Sql;

public class DqtContext : DbContext
{
    public DqtContext(DbContextOptions<DqtContext> options)
        : base(options)
    {
    }

    public static DqtContext Create(string connectionString) =>
        new DqtContext(CreateOptions(connectionString));

    public DbSet<TrnRequest> TrnRequests => Set<TrnRequest>();

    public DbSet<EntityChangesJournal> EntityChangesJournals => Set<EntityChangesJournal>();

    public DbSet<Person> People => Set<Person>();

    public static void ConfigureOptions(DbContextOptionsBuilder optionsBuilder, string connectionString)
    {
        if (connectionString != null)
        {
            optionsBuilder.UseNpgsql(connectionString);
        }
        else
        {
            optionsBuilder.UseNpgsql();
        }

        optionsBuilder
            .UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DqtContext).Assembly);
    }

    private static DbContextOptions<DqtContext> CreateOptions(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DqtContext>();
        ConfigureOptions(optionsBuilder, connectionString);
        return optionsBuilder.Options;
    }
}

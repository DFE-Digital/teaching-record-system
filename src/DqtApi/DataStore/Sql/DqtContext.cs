using Microsoft.EntityFrameworkCore;

namespace DqtApi.DataStore.Sql
{
    public class DqtContext : DbContext
    {
        public DqtContext(DbContextOptions<DqtContext> options)
            : base(options)
        {
        }

        public DqtContext(string connectionString)
            : this(CreateOptions(connectionString))
        {
        }

        public static void ConfigureOptions(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            optionsBuilder
                .UseNpgsql(connectionString)
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
}

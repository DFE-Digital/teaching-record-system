using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DqtApi.DataStore.Sql
{
    public class DqtDesignTimeDbContextFactory : IDesignTimeDbContextFactory<DqtContext>
    {
        public DqtContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<DqtContext>()
                .UseNpgsql()
                .UseSnakeCaseNamingConvention()
                .Options;

            return new DqtContext(options);
        }
    }
}

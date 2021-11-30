using Microsoft.EntityFrameworkCore;

namespace DqtApi.DataStore.Sql
{
    public class DqtContext : DbContext
    {
        public DqtContext(DbContextOptions<DqtContext> options)
            : base(options)
        {
        }
    }
}

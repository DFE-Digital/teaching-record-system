using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Api.Tests;

public class ExecuteSqlSetupAttribute : TestSetupAttribute
{
    public ExecuteSqlSetupAttribute(string sql)
    {
        Sql = sql;
    }

    public string Sql { get; }

    public override async Task Execute(TestInfo testInfo)
    {
        using var dbContext = testInfo.TestServices.GetRequiredService<TrsDbContext>();
        await dbContext.Database.ExecuteSqlRawAsync(Sql);
    }
}

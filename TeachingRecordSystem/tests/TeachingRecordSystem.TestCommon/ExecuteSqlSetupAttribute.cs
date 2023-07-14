using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.TestFramework;

namespace TeachingRecordSystem.TestCommon;

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

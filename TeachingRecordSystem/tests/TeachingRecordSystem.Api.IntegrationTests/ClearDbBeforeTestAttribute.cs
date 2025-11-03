using System.Reflection;
using Xunit.v3;

namespace TeachingRecordSystem.Api.IntegrationTests;

public class ClearDbBeforeTestAttribute : TeachingRecordSystem.TestCommon.ClearDbBeforeTestAttribute
{
    public override void Before(MethodInfo methodUnderTest, IXunitTest test)
    {
        base.Before(methodUnderTest, test);

        using var dbContext = DbHelper.Instance.DbContextFactory.CreateDbContext();
        HostFixture.AddApplicationUsers(dbContext);
    }
}

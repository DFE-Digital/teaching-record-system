namespace TeachingRecordSystem.Api.IntegrationTests;

public class ClearDbBeforeTestAttribute : TeachingRecordSystem.TestCommon.ClearDbBeforeTestAttribute
{
    public override async Task ClearAsync()
    {
        await base.ClearAsync();

        using var dbContext = DbHelper.Instance.DbContextFactory.CreateDbContext();
        HostFixture.EnsureApplicationUsers(dbContext);
    }
}

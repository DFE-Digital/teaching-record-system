using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.EndToEndTests.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public abstract class TestBase
{
    public TestBase(HostFixture hostFixture)
    {
        HostFixture = hostFixture;

        SetCurrentUser(TestUsers.Administrator);
    }

    public HostFixture HostFixture { get; }

    public TestData TestData => HostFixture.Services.GetRequiredService<TestData>();

    public virtual async Task<T> WithDbContext<T>(Func<TrsDbContext, Task<T>> action)
    {
        var dbContextFactory = HostFixture.Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await action(dbContext);
    }

    protected void SetCurrentUser(User user)
    {
        var currentUserProvider = HostFixture.Services.GetRequiredService<CurrentUserProvider>();
        currentUserProvider.CurrentUser = user;
    }

    public static string TextSelector(string? text) => $":text(\"{text}\")";

    public static string TextIsSelector(string? text) => $":text-is(\"{text}\")";

    public static string HasTextSelector(string? text) => $":has-text(\"{text}\")";
}

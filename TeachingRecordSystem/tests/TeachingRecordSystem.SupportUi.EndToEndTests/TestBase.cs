using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.EndToEndTests.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.EndToEndTests;

[SharedDependenciesDataSource]
[RetryOnCI(3)]
[NotInParallel]
public abstract class TestBase(HostFixture hostFixture)
{
    [Before(Test)]
    public void SetInitialUser() => SetCurrentUser(TestUsers.Administrator);

    protected HostFixture HostFixture { get; } = hostFixture;

    protected TestData TestData => HostFixture.Services.GetRequiredService<TestData>();

    protected async Task<T> WithDbContext<T>(Func<TrsDbContext, Task<T>> action)
    {
        var dbContextFactory = HostFixture.Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await action(dbContext);
    }

    protected Task WithDbContext(Func<TrsDbContext, Task> action) =>
        WithDbContext(async dbContext =>
        {
            await action(dbContext);
            return 0;
        });

    protected void SetCurrentUser(User user)
    {
        var currentUserProvider = HostFixture.Services.GetRequiredService<CurrentUserProvider>();
        currentUserProvider.CurrentUser = user;
    }

    public static string TextSelector(string? text) => $":text(\"{text}\")";

    public static string TextIsSelector(string? text) => $":text-is(\"{text}\")";

    public static string HasTextSelector(string? text) => $":has-text(\"{text}\")";
}

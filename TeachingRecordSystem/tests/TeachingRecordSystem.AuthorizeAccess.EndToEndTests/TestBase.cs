using TeachingRecordSystem.AuthorizeAccess.EndToEndTests.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.AuthorizeAccess.EndToEndTests;

public abstract class TestBase(HostFixture hostFixture)
{
    public HostFixture HostFixture { get; } = hostFixture;

    public IClock Clock => HostFixture.Services.GetRequiredService<IClock>();

    public TestData TestData => HostFixture.Services.GetRequiredService<TestData>();

    public virtual ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public virtual ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public virtual async Task<T> WithDbContext<T>(Func<TrsDbContext, Task<T>> action)
    {
        var dbContextFactory = HostFixture.Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await action(dbContext);
    }

    public void SetCurrentOneLoginUser(OneLoginUserInfo user)
    {
        var currentUserProvider = HostFixture.Services.GetRequiredService<OneLoginCurrentUserProvider>();
        currentUserProvider.CurrentUser = user;
    }

    public static string TextSelector(string? text) => $":text(\"{text}\")";

    public static string TextIsSelector(string? text) => $":text-is(\"{text}\")";

    public static string HasTextSelector(string? text) => $":has-text(\"{text}\")";
}

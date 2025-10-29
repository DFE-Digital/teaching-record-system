using TeachingRecordSystem.AuthorizeAccess.EndToEndTests.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.AuthorizeAccess.EndToEndTests;

public abstract class TestBase(HostFixture hostFixture)
{
    public static string TextSelector(string? text) => $":text(\"{text}\")";

    public static string TextIsSelector(string? text) => $":text-is(\"{text}\")";

    public static string HasTextSelector(string? text) => $":has-text(\"{text}\")";

    protected HostFixture HostFixture { get; } = hostFixture;

    protected IClock Clock => HostFixture.Services.GetRequiredService<IClock>();

    protected IDbContextFactory<TrsDbContext> DbContextFactory => HostFixture.Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    protected TestData TestData => HostFixture.Services.GetRequiredService<TestData>();

    protected void SetCurrentOneLoginUser(OneLoginUserInfo user)
    {
        var currentUserProvider = HostFixture.Services.GetRequiredService<OneLoginCurrentUserProvider>();
        currentUserProvider.CurrentUser = user;
    }

    protected Task<T> WithDbContextAsync<T>(Func<TrsDbContext, Task<T>> action) =>
        DbContextFactory.WithDbContextAsync(action);

    protected Task WithDbContextAsync(Func<TrsDbContext, Task> action) =>
        DbContextFactory.WithDbContextAsync(action);
}

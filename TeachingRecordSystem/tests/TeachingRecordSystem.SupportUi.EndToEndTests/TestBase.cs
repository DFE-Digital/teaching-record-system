using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.EndToEndTests.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public abstract class TestBase
{
    protected TestBase(HostFixture hostFixture)
    {
        HostFixture = hostFixture;

        SetCurrentUser(TestUsers.Administrator);
    }

    protected HostFixture HostFixture { get; }

    protected IDbContextFactory<TrsDbContext> DbContextFactory => HostFixture.Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    protected TestData TestData => HostFixture.Services.GetRequiredService<TestData>();

    public static string TextSelector(string? text) => $":text(\"{text}\")";

    public static string TextIsSelector(string? text) => $":text-is(\"{text}\")";

    public static string HasTextSelector(string? text) => $":has-text(\"{text}\")";

    protected void SetCurrentUser(User user)
    {
        var currentUserProvider = HostFixture.Services.GetRequiredService<CurrentUserProvider>();
        currentUserProvider.CurrentUser = user;
    }

    protected Task<T> WithDbContextAsync<T>(Func<TrsDbContext, Task<T>> action) =>
        DbContextFactory.WithDbContextAsync(action);

    protected Task WithDbContextAsync(Func<TrsDbContext, Task> action) =>
        DbContextFactory.WithDbContextAsync(action);
}

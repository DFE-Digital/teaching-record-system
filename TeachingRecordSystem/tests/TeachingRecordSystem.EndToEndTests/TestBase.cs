using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.EndToEndTests.Infrastructure.Security;

namespace TeachingRecordSystem.EndToEndTests;

public abstract class TestBase(HostFixture hostFixture)
{
    protected string ApiBaseUrl => HostFixture.ApiBaseUrl;
    protected string AuthorizeAccessBaseUrl => HostFixture.AuthorizeAccessBaseUrl;
    protected string SupportUiBaseUrl => HostFixture.SupportUiBaseUrl;

    protected HostFixture HostFixture { get; } = hostFixture;

    protected TimeProvider TimeProvider => HostFixture.TimeProvider;

    protected IDbContextFactory<TrsDbContext> DbContextFactory => HostFixture.DbContextFactory;

    protected TestData TestData => HostFixture.TestData;

    protected void SetCurrentOneLoginUser(OneLoginUserInfo user)
    {
        var currentUserProvider = HostFixture.AuthorizeAccessHostServices.GetRequiredService<OneLoginCurrentUserProvider>();
        currentUserProvider.CurrentUser = user;
    }

    protected Task<T> WithDbContextAsync<T>(Func<TrsDbContext, Task<T>> action) =>
        DbContextFactory.WithDbContextAsync(action);

    protected Task WithDbContextAsync(Func<TrsDbContext, Task> action) =>
        DbContextFactory.WithDbContextAsync(action);
}

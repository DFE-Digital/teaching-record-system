using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.EndToEndTests.Infrastructure.Security;
using TeachingRecordSystem.EndToEndTests.Infrastructure.Webhooks;

namespace TeachingRecordSystem.EndToEndTests;

public abstract class TestBase
{
    protected TestBase(HostFixture hostFixture)
    {
        HostFixture = hostFixture;

        WebhookMessageRecorder.Clear();
    }

    protected string ApiBaseUrl => HostFixture.ApiBaseUrl;
    protected string AuthorizeAccessBaseUrl => HostFixture.AuthorizeAccessBaseUrl;
    protected string SupportUiBaseUrl => HostFixture.SupportUiBaseUrl;

    protected HostFixture HostFixture { get; }

    protected TimeProvider TimeProvider => HostFixture.TimeProvider;

    protected IDbContextFactory<TrsDbContext> DbContextFactory => HostFixture.DbContextFactory;

    protected TestData TestData => HostFixture.TestData;

    protected WebhookMessageRecorder WebhookMessageRecorder => HostFixture.WebhookMessageRecorder;

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

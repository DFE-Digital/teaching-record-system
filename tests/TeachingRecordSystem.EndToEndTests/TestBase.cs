using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.EndToEndTests.Infrastructure.Security;
using TeachingRecordSystem.EndToEndTests.Infrastructure.Webhooks;

namespace TeachingRecordSystem.EndToEndTests;

public abstract class TestBase
{
    protected TestBase(HostFixture hostFixture)
    {
        HostFixture = hostFixture;

        WebhookMessageRecorder.Clear();
        SetCurrentUser(TestUsers.Administrator);
    }

    protected string ApiBaseUrl => HostFixture.ApiBaseUrl;
    protected string AuthorizeAccessBaseUrl => HostFixture.AuthorizeAccessBaseUrl;
    protected string SupportUiBaseUrl => HostFixture.SupportUiBaseUrl;

    protected HostFixture HostFixture { get; }

    protected TimeProvider TimeProvider => HostFixture.TimeProvider;

    protected IDbContextFactory<TrsDbContext> DbContextFactory => HostFixture.DbContextFactory;

    protected TestData TestData => HostFixture.TestData;

    protected WebhookMessageRecorder WebhookMessageRecorder => HostFixture.WebhookMessageRecorder;

    public static string TextSelector(string? text) => $":text(\"{text?.Replace("\"", "\\\"")}\")";

    public static string TextIsSelector(string? text) => $":text-is(\"{text?.Replace("\"", "\\\"")}\")";

    public static string HasTextSelector(string? text) => $":has-text(\"{text?.Replace("\"", "\\\"")}\")";

    public static string LinkHrefContains(string hrefPart) => $"a[href*=\"{hrefPart}\"]";

    protected void SetCurrentOneLoginUser(OneLoginUserInfo user)
    {
        var currentUserProvider = HostFixture.AuthorizeAccessHostServices.GetRequiredService<OneLoginCurrentUserProvider>();
        currentUserProvider.CurrentUser = user;
    }

    protected void SetCurrentUser(User user)
    {
        var currentUserProvider = HostFixture.SupportUiHostServices.GetRequiredService<CurrentUserProvider>();
        currentUserProvider.CurrentUser = user;
    }

    protected Task<T> WithDbContextAsync<T>(Func<TrsDbContext, Task<T>> action) =>
        DbContextFactory.WithDbContextAsync(action);

    protected Task WithDbContextAsync(Func<TrsDbContext, Task> action) =>
        DbContextFactory.WithDbContextAsync(action);
}

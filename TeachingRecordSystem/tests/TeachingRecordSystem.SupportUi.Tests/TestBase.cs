using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.SupportUi.Tests.Infrastructure;
using TeachingRecordSystem.SupportUi.Tests.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Tests;

public abstract class TestBase
{
    private readonly TestScopedServices _testServices;

    protected TestBase(HostFixture hostFixture)
    {
        HostFixture = hostFixture;

        HostFixture.EventObserver.Init();
        _testServices = TestScopedServices.Reset();
        SetCurrentUser(TestUsers.Administrator);

        HttpClient = hostFixture.CreateClient(new()
        {
            AllowAutoRedirect = false
        });
    }

    public HostFixture HostFixture { get; }

    public CaptureEventObserver EventObserver => HostFixture.EventObserver;

    public Mock<IDataverseAdapter> DataverseAdapterMock => _testServices.DataverseAdapterMock;

    public TestableClock Clock => _testServices.Clock;

    public HttpClient HttpClient { get; }

    protected void SetCurrentUser(User user)
    {
        var currentUserProvider = HostFixture.Services.GetRequiredService<CurrentUserProvider>();
        currentUserProvider.CurrentUser = user;
    }

    public virtual async Task<T> WithDbContext<T>(Func<TrsDbContext, Task<T>> action)
    {
        var dbContextFactory = HostFixture.Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await action(dbContext);
    }

    public virtual Task WithDbContext(Func<TrsDbContext, Task> action) =>
        WithDbContext(async dbContext =>
        {
            await action(dbContext);
            return 0;
        });
}

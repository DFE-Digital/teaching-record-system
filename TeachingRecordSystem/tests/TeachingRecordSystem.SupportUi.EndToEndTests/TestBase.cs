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

    protected void SetCurrentUser(User user)
    {
        var currentUserProvider = HostFixture.Services.GetRequiredService<CurrentUserProvider>();
        currentUserProvider.CurrentUser = user;
    }
}

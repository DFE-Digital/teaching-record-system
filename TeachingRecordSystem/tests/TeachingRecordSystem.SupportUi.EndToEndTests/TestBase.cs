using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.EndToEndTests.Infrastructure.Security;
using TeachingRecordSystem.TestCommon;

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

    protected void SetCurrentUser(User user)
    {
        var currentUserProvider = HostFixture.Services.GetRequiredService<CurrentUserProvider>();
        currentUserProvider.CurrentUser = user;
    }
}

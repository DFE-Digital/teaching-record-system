using TeachingRecordSystem.SupportUi.Infrastructure.FormFlow;
using TeachingRecordSystem.SupportUi.Tests.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Tests.Infrastructure.FormFlow;

public class TestUserCurrentUserIdProvider : ICurrentUserIdProvider
{
    private readonly CurrentUserProvider _currentUserProvider;

    public TestUserCurrentUserIdProvider(CurrentUserProvider currentUserProvider)
    {
        _currentUserProvider = currentUserProvider;
    }

    public Guid GetCurrentUserId() =>
        _currentUserProvider.CurrentUser?.UserId ?? throw new InvalidOperationException("No current user.");
}

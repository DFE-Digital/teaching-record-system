using TeachingRecordSystem.SupportUi.Infrastructure.FormFlow;
using TeachingRecordSystem.SupportUi.Tests.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Tests.Infrastructure.FormFlow;

public class TestUserCurrentUserIdProvider(CurrentUserProvider currentUserProvider) : ICurrentUserIdProvider
{
    public string GetCurrentUserId() =>
        currentUserProvider.CurrentUser?.UserId.ToString() ?? throw new InvalidOperationException("No current user.");
}

using TeachingRecordSystem.SupportUi.Infrastructure.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Infrastructure.FormFlow;

public class DummyCurrentUserIdProvider : ICurrentUserIdProvider
{
    public string GetCurrentUserId() => Guid.Empty.ToString();
}

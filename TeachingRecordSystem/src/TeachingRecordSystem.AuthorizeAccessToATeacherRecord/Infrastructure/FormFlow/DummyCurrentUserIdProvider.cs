using TeachingRecordSystem.SupportUi.Infrastructure.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccessToATeacherRecord.Infrastructure.FormFlow;

public class DummyCurrentUserIdProvider : ICurrentUserIdProvider
{
    public string GetCurrentUserId() => Guid.Empty.ToString();
}

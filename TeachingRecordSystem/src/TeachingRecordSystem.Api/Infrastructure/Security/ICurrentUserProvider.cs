namespace TeachingRecordSystem.Api.Infrastructure.Security;

public interface ICurrentUserProvider
{
    Guid GetCurrentApplicationUserId();
}

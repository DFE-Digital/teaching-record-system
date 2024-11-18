namespace TeachingRecordSystem.Api.Infrastructure.Security;

public interface ICurrentUserProvider
{
    (Guid UserId, string Name) GetCurrentApplicationUser();
}

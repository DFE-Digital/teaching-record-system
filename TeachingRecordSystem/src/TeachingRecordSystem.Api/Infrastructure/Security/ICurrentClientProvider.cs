namespace TeachingRecordSystem.Api.Infrastructure.Security;

public interface ICurrentClientProvider
{
    string GetCurrentClientId();
}

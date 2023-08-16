namespace TeachingRecordSystem.Api.Infrastructure.Security;

public interface IApiClientRepository
{
    ApiClient? GetClientByKey(string apiKey);
}

#nullable disable
namespace QualifiedTeachersApi.Security;

public interface IApiClientRepository
{
    ApiClient GetClientByKey(string apiKey);
}

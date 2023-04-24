#nullable disable

namespace QualifiedTeachersApi.Infrastructure.Security;

public interface IApiClientRepository
{
    ApiClient GetClientByKey(string apiKey);
}

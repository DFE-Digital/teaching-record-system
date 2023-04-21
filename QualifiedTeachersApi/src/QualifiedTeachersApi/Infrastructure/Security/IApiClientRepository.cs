#nullable disable
using QualifiedTeachersApi;

namespace QualifiedTeachersApi.Infrastructure.Security;

public interface IApiClientRepository
{
    ApiClient GetClientByKey(string apiKey);
}

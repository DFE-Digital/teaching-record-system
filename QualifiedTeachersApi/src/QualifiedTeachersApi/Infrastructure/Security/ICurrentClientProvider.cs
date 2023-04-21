#nullable disable
using QualifiedTeachersApi;

namespace QualifiedTeachersApi.Infrastructure.Security;

public interface ICurrentClientProvider
{
    string GetCurrentClientId();
}

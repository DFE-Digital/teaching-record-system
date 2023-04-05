#nullable disable
namespace QualifiedTeachersApi.Security;

public interface ICurrentClientProvider
{
    string GetCurrentClientId();
}

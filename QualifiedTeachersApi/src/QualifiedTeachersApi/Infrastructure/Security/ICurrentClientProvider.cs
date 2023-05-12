namespace QualifiedTeachersApi.Infrastructure.Security;

public interface ICurrentClientProvider
{
    string GetCurrentClientId();
}

namespace TeachingRecordSystem.Api.Infrastructure.Security;

public static class AuthorizationPolicies
{
    public const string ApiKey = nameof(ApiKey);
    public const string IdentityUserWithTrn = nameof(IdentityUserWithTrn);
    public const string GetPerson = nameof(GetPerson);
    public const string UpdatePerson = nameof(UpdatePerson);
    public const string UpdateNpq = nameof(UpdateNpq);
    public const string UnlockPerson = nameof(UnlockPerson);
    public const string CreateTrn = nameof(CreateTrn);
    public const string AssignQtls = nameof(AssignQtls);

    public static bool IsApiKeyAuthentication(string policy)
    {
        switch (policy)
        {
            case AssignQtls:
            case ApiKey:
            case GetPerson:
            case UpdatePerson:
            case UpdateNpq:
            case UnlockPerson:
            case CreateTrn:
                return true;
            default:
                return false;
        }
    }
}

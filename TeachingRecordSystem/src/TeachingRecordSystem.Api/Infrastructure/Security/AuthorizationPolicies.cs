namespace TeachingRecordSystem.Api.Infrastructure.Security;

public static class AuthorizationPolicies
{
    public const string ApiKey = nameof(ApiKey);
    public const string IdentityUserWithTrn = nameof(IdentityUserWithTrn);
    public const string Hangfire = nameof(Hangfire);
    public const string GetPerson = nameof(GetPerson);
    public const string UpdatePerson = nameof(UpdatePerson);
    public const string UpdateNpq = nameof(UpdateNpq);
    public const string UnlockPerson = nameof(UnlockPerson);

    public static bool IsApiKeyAuthentication(string policy)
    {
        switch (policy)
        {
            case ApiKey:
            case Hangfire:
            case GetPerson:
            case UpdatePerson:
            case UpdateNpq:
            case UnlockPerson:
                return true;
            case IdentityUserWithTrn:
            default:
                return false;

        }
    }
}

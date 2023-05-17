namespace QualifiedTeachersApi.Infrastructure.Security;

public static class AuthorizationPolicies
{
    public const string ApiKey = nameof(ApiKey);
    public const string IdentityUserWithTrn = nameof(IdentityUserWithTrn);
    public const string Hangfire = nameof(Hangfire);
}

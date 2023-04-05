#nullable disable
namespace QualifiedTeachersApi.Security;

public static class AuthorizationPolicies
{
    public const string ApiKey = nameof(ApiKey);
    public const string IdentityUserWithTrn = nameof(IdentityUserWithTrn);
}

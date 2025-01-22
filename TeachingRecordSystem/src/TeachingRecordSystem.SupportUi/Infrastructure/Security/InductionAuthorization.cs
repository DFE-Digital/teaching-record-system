using Microsoft.AspNetCore.Authorization;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public static class InductionAuthorization
{
    public static AuthorizationBuilder AddInductionPolicies(this AuthorizationBuilder builder) => builder
        .AddPolicy(
            AuthorizationPolicies.InductionReadWrite,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(UserRoles.InductionReadWrite, UserRoles.Administrator));
}

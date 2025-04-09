using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public static class InductionAuthorization
{
    public static AuthorizationBuilder AddInductionPolicies(this AuthorizationBuilder builder)
    {
        builder.AddPolicy(
            AuthorizationPolicies.InductionReadWrite,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new InductionReadWriteRequirement()));

        builder.Services
            .AddSingleton<IAuthorizationHandler, InductionReadWriteAuthorizationHandler>()
            // AuthorizationHandler for Legacy user roles, delete when existing users have been migrated to new user roles.
            .AddSingleton<IAuthorizationHandler, LegacyInductionReadWriteAuthorizationHandler>();

        return builder;
    }
}

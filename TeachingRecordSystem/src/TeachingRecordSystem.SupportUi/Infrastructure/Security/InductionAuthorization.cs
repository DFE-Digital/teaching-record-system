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
            AuthorizationPolicies.InductionView,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new InductionViewRequirement()));

        builder.AddPolicy(
            AuthorizationPolicies.InductionEdit,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new InductionEditRequirement()));

        builder.Services
            .AddSingleton<IAuthorizationHandler, InductionViewAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, InductionEditAuthorizationHandler>()
            // AuthorizationHandler for Legacy user roles, delete when existing users have been migrated to new user roles.
            .AddSingleton<IAuthorizationHandler, LegacyInductionViewAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, LegacyInductionEditAuthorizationHandler>();

        return builder;
    }
}

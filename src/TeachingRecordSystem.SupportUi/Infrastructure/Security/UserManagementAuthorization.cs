using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public static class UserManagementAuthorization
{
    public static AuthorizationBuilder AddUserManagementPolicies(this AuthorizationBuilder builder)
    {
        builder.AddPolicy(
            AuthorizationPolicies.UserManagement,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new UserManagementRequirement()));

        builder.Services
            .AddSingleton<IAuthorizationHandler, UserManagementAuthorizationHandler>();

        return builder;
    }
}

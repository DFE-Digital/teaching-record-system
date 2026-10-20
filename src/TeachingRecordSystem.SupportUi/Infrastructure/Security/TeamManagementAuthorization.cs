using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public static class TeamManagementAuthorization
{
    public static AuthorizationBuilder AddTeamManagementPolicies(this AuthorizationBuilder builder)
    {
        builder.AddPolicy(
            AuthorizationPolicies.TeamManagement,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new TeamManagementRequirement()));

        builder.Services
            .AddSingleton<IAuthorizationHandler, TeamManagementAuthorizationHandler>();

        return builder;
    }
}

using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;

public class TeamManagementAuthorizationHandler : AuthorizationHandler<TeamManagementRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TeamManagementRequirement requirement)
    {
        if (context.User.HasMinimumPermission(new(UserPermissionTypes.ManageTeams, UserPermissionLevel.Edit)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

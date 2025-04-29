using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;

public class ChangeManagementAuthorizationHandler : AuthorizationHandler<ChangeManagementRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ChangeManagementRequirement requirement)
    {
        if (context.User.HasMinimumPermission(new(UserPermissionTypes.SupportTasks, UserPermissionLevel.Edit)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

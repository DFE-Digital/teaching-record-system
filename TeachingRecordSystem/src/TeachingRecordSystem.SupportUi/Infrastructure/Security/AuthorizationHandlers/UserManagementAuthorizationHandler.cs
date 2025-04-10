using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;

public class UserManagementAuthorizationHandler : AuthorizationHandler<UserManagementRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserManagementRequirement requirement)
    {
        if (context.User.HasMinimumPermission(new(UserPermissionTypes.ManageUsers, UserPermissionLevel.Edit)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

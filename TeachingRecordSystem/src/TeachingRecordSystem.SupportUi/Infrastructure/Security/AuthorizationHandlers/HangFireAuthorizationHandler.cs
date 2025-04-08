using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;

public class HangFireAuthorizationHandler : AuthorizationHandler<HangFireRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HangFireRequirement requirement)
    {
        if (context.User.HasMinimumPermission(new(UserPermissionTypes.AdminArea, UserPermissionLevel.Edit)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

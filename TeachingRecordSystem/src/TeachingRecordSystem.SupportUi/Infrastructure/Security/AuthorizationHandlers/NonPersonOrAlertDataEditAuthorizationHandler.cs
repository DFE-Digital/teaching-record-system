using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;

public class NonPersonOrAlertDataEditAuthorizationHandler : AuthorizationHandler<NonPersonOrAlertDataEditRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, NonPersonOrAlertDataEditRequirement requirement)
    {
        if (context.User.HasMinimumPermission(new(UserPermissionTypes.NonPersonOrAlertData, UserPermissionLevel.Edit)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

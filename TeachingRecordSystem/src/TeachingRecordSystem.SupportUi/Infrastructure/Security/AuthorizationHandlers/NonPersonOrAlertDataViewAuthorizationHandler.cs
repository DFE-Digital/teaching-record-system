using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;

public class NonPersonOrAlertDataViewAuthorizationHandler : AuthorizationHandler<NonPersonOrAlertDataViewRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, NonPersonOrAlertDataViewRequirement requirement)
    {
        if (context.User.HasMinimumPermission(new(UserPermissionTypes.NonPersonOrAlertData, UserPermissionLevel.View)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers.Legacy;

/// <summary>
/// AuthorizationHandler for Legacy user roles, delete when existing users have been migrated to new user roles.
/// </summary>
public class LegacyNotesViewAuthorizationHandler : AuthorizationHandler<NotesViewRequirement, Guid>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, NotesViewRequirement requirement, Guid personId)
    {
        // If the user has been migrated to the new user roles, they may still have the legacy roles, so we need to
        // disregard them for this user as they may be different to their new role.
        if (context.User.HasBeenMigrated())
        {
            return Task.CompletedTask;
        }

        // The Notes tab was previously visible to all users, so if the user hasn't been migrated yet,
        // they should still see the tab.
        context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

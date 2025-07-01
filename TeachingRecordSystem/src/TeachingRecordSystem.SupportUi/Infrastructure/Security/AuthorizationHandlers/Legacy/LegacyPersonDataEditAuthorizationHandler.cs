using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers.Legacy;

/// <summary>
/// AuthorizationHandler for Legacy user roles, delete when existing users have been migrated to new user roles.
/// </summary>
public class LegacyPersonDataEditAuthorizationHandler : AuthorizationHandler<PersonDataEditRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PersonDataEditRequirement requirement)
    {
        // If the user has been migrated to the new user roles, they may still have the legacy roles, so we need to
        // disregard them for this user as they may be different to their new role.
        if (context.User.HasBeenMigrated())
        {
            return Task.CompletedTask;
        }

        // Personal details were not previously editable by any user, so if the user hasn't been migrated yet,
        // they should not be able to edit person data.
        // Note: This situation shouldn't arise as the Personal details create/edit functionality will only be enabled
        // once all users have been migrated to the new user roles.
        return Task.CompletedTask;
    }
}

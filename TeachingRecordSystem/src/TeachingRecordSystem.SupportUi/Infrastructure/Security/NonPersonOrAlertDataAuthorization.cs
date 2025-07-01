using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public static class NonPersonOrAlertDataAuthorization
{
    public static AuthorizationBuilder AddNonPersonOrAlertDataPolicies(this AuthorizationBuilder builder)
    {
        builder.AddPolicy(
            AuthorizationPolicies.NonPersonOrAlertDataView,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new NonPersonOrAlertDataViewRequirement()));

        builder.AddPolicy(
            AuthorizationPolicies.NonPersonOrAlertDataEdit,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new NonPersonOrAlertDataEditRequirement()));

        builder.Services
            .AddSingleton<IAuthorizationHandler, NonPersonOrAlertDataViewAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, NonPersonOrAlertDataEditAuthorizationHandler>()
            // AuthorizationHandler for Legacy user roles, delete when existing users have been migrated to new user roles.
            .AddSingleton<IAuthorizationHandler, LegacyNonPersonOrAlertDataViewAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, LegacyNonPersonOrAlertDataEditAuthorizationHandler>();

        return builder;
    }
}

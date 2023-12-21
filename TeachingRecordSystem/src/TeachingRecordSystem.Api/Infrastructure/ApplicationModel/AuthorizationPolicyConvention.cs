using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace TeachingRecordSystem.Api.Infrastructure.ApplicationModel;

public class AuthorizationPolicyConvention : IActionModelConvention
{
    public void Apply(ActionModel action)
    {
        // Skip controllers not defined in this assembly (e.g. test controllers)
        if (action.Controller.ControllerType.Assembly != typeof(ApiVersionConvention).Assembly)
        {
            return;
        }

        // V1 and V2 endpoints are always authenticated using an API key;
        // V3 endpoints should have policy explicitly specified

        if (!action.Controller.Properties.TryGetValue("Version", out var versionObj) || versionObj is not int version)
        {
            throw new InvalidOperationException($"{action.Controller.ControllerName} does not have a version assigned.");
        }

        if (!action.Attributes.Any(att => att is AuthorizeAttribute) &&
            !action.Controller.Attributes.Any(att => att is AuthorizeAttribute))
        {
            throw new InvalidOperationException(
                $"The {action.ActionName} action on the {action.Controller.ControllerName} controller does not have an authorize attribute assigned.");
        }
    }
}

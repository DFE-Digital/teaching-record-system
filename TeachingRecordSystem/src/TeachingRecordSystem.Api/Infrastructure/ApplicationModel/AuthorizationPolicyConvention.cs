using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace TeachingRecordSystem.Api.Infrastructure.ApplicationModel;

public class AuthorizationPolicyConvention : IActionModelConvention
{
    public void Apply(ActionModel action)
    {
        // V1 and V2 endpoints are always authenticated using an API key;
        // V3 endpoints should have policy explicitly specified

        if (!action.Controller.Properties.TryGetValue("Version", out var versionObj) || versionObj is not int version)
        {
            if (action.Controller.ControllerType.Namespace?.Contains("Tests") == true)
            {
                return;
            }

            throw new Exception("Controller does not have a version assigned.");
        }

        if (!action.Attributes.Any(att => att is AuthorizeAttribute) &&
            !action.Controller.Attributes.Any(att => att is AuthorizeAttribute))
        {
            throw new Exception("Action does not have assigned authorize attribute");
        }
    }
}

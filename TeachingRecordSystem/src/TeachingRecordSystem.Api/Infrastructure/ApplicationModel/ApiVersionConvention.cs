using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using TeachingRecordSystem.Api.Infrastructure.OpenApi;

namespace TeachingRecordSystem.Api.Infrastructure.ApplicationModel;

public class ApiVersionConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        // Skip controllers not defined in this assembly (e.g. test controllers)
        if (controller.ControllerType.Assembly != typeof(ApiVersionConvention).Assembly)
        {
            return;
        }

        var controllerNamespace = controller.ControllerType.Namespace ??
            throw new InvalidOperationException($"{controller.ControllerType} is not defined within a namespace.");

        var projectRelativeNamespace = controllerNamespace[(typeof(ApiVersionConvention).Assembly.GetName().Name + ".").Length..];
        var nsParts = projectRelativeNamespace.Split('.');

        var namespaceMajorVersion = nsParts[0];

        if (namespaceMajorVersion[0] != 'V' || !int.TryParse(namespaceMajorVersion.TrimStart('V'), out var version))
        {
            throw new InvalidOperationException($"{controller.ControllerName} is not defined in a properly-versioned namespace.");
        }

        // Group name is used to partition the operations by version into different swagger docs
        controller.ApiExplorer.GroupName = OpenApiDocumentHelper.GetDocumentName(version);

        // Store the version so other components can easily query it later
        controller.Properties.Add("Version", version);

        // A V1 operation gets a /v1 route prefix, V2 operation a /v2 route prefix etc.
        {
            var routePrefix = new AttributeRouteModel(new RouteAttribute($"v{version}"));

            foreach (var selector in controller.Selectors)
            {
                selector.AttributeRouteModel = selector.AttributeRouteModel != null ?
                    AttributeRouteModel.CombineAttributeRouteModel(routePrefix, selector.AttributeRouteModel) :
                    routePrefix;
            }
        }

        // V3 operations should exist within a minor versioned namespace
        if (version == 3)
        {
            var namespaceMinorVersion = nsParts.Length >= 2 ? nsParts[1] : null;

            if (namespaceMinorVersion is null || namespaceMinorVersion[0] != 'V')
            {
                throw new InvalidOperationException($"{controller.ControllerName} is not defined within a properly-versioned namespace.");
            }

            var minorVersion = namespaceMinorVersion[1..];

            if (!Constants.AllV3MinorVersions.Contains(minorVersion))
            {
                throw new InvalidOperationException(
                    $"{controller.ControllerName} minor version '{minorVersion}' has not been defined in {nameof(Constants)}.{nameof(Constants.AllV3MinorVersions)}.");
            }

            controller.Properties.Add("MinorVersion", minorVersion);
        }
    }
}

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using TeachingRecordSystem.Api.Infrastructure.Features;

namespace TeachingRecordSystem.Api.Infrastructure.ApplicationModel;

public class BackFillVersionedEndpointsConvention : IApplicationModelConvention
{
    public void Apply(Microsoft.AspNetCore.Mvc.ApplicationModels.ApplicationModel application)
    {
        // Extract the route and HTTP method for each action then look for the corresponding ActionModel for any later versions.
        // If there is not one present, add additional ApiMinorVersionMetadata to this action so that requests for those later versions
        // are routed to this version.

        var versionedEndpoints = application.Controllers
            .Where(c => c.ControllerType.Assembly == typeof(BackFillVersionedEndpointsConvention).Assembly)
            .Select(c => (Controller: c, MinorVersion: c.Properties.TryGetValue(Constants.DeclaredMinorVersionPropertyKey, out var minorVersion) ? (string)minorVersion! : null))
            .Where(c => c.MinorVersion is not null)
            .SelectMany(c => c.Controller.Actions.Select(a => (Action: a, c.Controller, MinorVersion: c.MinorVersion!)))
            .Select(m =>
            {
                if (m.Action.Controller.Selectors.Count != 1)
                {
                    throw new NotSupportedException("Controllers with multiple selectors are not supported.");
                }

                var controllerSelectorModel = m.Controller.Selectors.Single();
                var actionSelectorModel = m.Action.Selectors.Single();

                var routeTemplate = AttributeRouteModel.CombineTemplates(controllerSelectorModel.AttributeRouteModel?.Template, actionSelectorModel.AttributeRouteModel?.Template);

                var httpMethod = actionSelectorModel.ActionConstraints.OfType<HttpMethodActionConstraint>().SingleOrDefault()?.HttpMethods.SingleOrDefault() ??
                    throw new NotSupportedException($"The {m.Action.ActionName} action on the {m.Action.Controller.ControllerName} controller does not have exactly one HTTP method assigned.");

                return (m.Action, m.Controller, m.MinorVersion, RouteTemplate: routeTemplate, HttpMethod: httpMethod);
            })
            .ToArray();

        var sortedVersions = VersionRegistry.AllV3MinorVersions.Order().ToArray();

        foreach (var ep in versionedEndpoints)
        {
            if (ep.Action.Attributes.OfType<RemovesFromApiAttribute>().Any())
            {
                ep.Controller.Actions.Remove(ep.Action);
            }

            var acceptedVersions = new List<string>()
            {
                ep.MinorVersion
            };

            var laterVersions = sortedVersions.Where(v => string.Compare(v, ep.MinorVersion) > 0);
            foreach (var version in laterVersions)
            {
                var epForThisVersion = versionedEndpoints.SingleOrDefault(
                    v => v.MinorVersion == version && v.RouteTemplate == ep.RouteTemplate && v.HttpMethod == ep.HttpMethod);

                if (epForThisVersion is { Action: null })
                {
                    acceptedVersions.Add(version);
                }
                else
                {
                    break;
                }
            }

            foreach (var selector in ep.Action.Selectors)
            {
                selector.ActionConstraints.Add(new MatchApiMinorVersionActionConstraint(acceptedVersions));
            }

            // Stash away the minor versions this endpoint supports to aid Swagger API generation later
            ep.Action.Properties.Add(Constants.MinorVersionsPropertyKey, new ApiMinorVersionsMetadata(acceptedVersions));
        }
    }

    private class MatchApiMinorVersionActionConstraint(IEnumerable<string> acceptedVersions) : IActionConstraint
    {
        public int Order => 0;

        public bool Accept(ActionConstraintContext context)
        {
            var requestedVersionFeature = context.RouteContext.HttpContext.Features.GetRequiredFeature<RequestedVersionFeature>();
            var requestEffectiveMinorVersion = requestedVersionFeature.EffectiveMinorVersion;
            return acceptedVersions.Contains(requestEffectiveMinorVersion);
        }
    }
}

public sealed class ApiMinorVersionsMetadata(IEnumerable<string> minorVersions)
{
    public IReadOnlyCollection<string> MinorVersions { get; } = minorVersions.ToArray();
}

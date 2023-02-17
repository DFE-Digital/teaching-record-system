using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace QualifiedTeachersApi
{
    public class ApiVersionConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            var controllerNamespace = controller.ControllerType.Namespace;
            var namespaceVersion = controllerNamespace.Split('.')[1];

            if (namespaceVersion[0] == 'V' && int.TryParse(namespaceVersion.TrimStart('V'), out var version))
            {
                ApplyGroupName();
                ApplyRoutePrefix();

                // Group name is used to partition the operations by version into different swagger docs
                void ApplyGroupName() => controller.ApiExplorer.GroupName = $"v{version}";

                // A V1 operation gets a /v1 route prefix, V2 operation a /v2 route prefix etc.
                void ApplyRoutePrefix()
                {
                    var routePrefix = new AttributeRouteModel(new RouteAttribute($"v{version}"));

                    foreach (var selector in controller.Selectors)
                    {
                        selector.AttributeRouteModel = selector.AttributeRouteModel != null ?
                            AttributeRouteModel.CombineAttributeRouteModel(routePrefix, selector.AttributeRouteModel) :
                            routePrefix;
                    }
                }
            }
        }
    }
}

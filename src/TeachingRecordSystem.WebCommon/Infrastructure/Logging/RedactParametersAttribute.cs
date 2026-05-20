using Microsoft.AspNetCore.Mvc.ApplicationModels;
using TeachingRecordSystem.WebCommon.Infrastructure.Logging;

namespace TeachingRecordSystem;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RedactParametersAttribute(params string[] parameterNames) : Attribute, IActionModelConvention, IPageApplicationModelConvention
{
    public string[] ParameterNames { get; } = parameterNames;

    void IActionModelConvention.Apply(ActionModel action)
    {
        foreach (var selector in action.Selectors)
        {
            UpdateEndpointMetadata(selector.EndpointMetadata);
        }
    }

    void IPageApplicationModelConvention.Apply(PageApplicationModel model) => UpdateEndpointMetadata(model.EndpointMetadata);

    private void UpdateEndpointMetadata(IList<object> endpointMetadata)
    {
        var metadata = endpointMetadata.OfType<RedactedParametersMetadata>().SingleOrDefault();
        if (metadata is null)
        {
            metadata = new();
            endpointMetadata.Add(metadata);
        }

        foreach (var parameterName in ParameterNames)
        {
            metadata.ParameterNames.Add(parameterName);
        }
    }
}

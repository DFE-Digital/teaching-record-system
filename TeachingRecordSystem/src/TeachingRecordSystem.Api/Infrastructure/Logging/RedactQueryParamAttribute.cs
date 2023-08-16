using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace TeachingRecordSystem.Api.Infrastructure.Logging;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class RedactQueryParamAttribute : Attribute, IActionModelConvention
{
    public RedactQueryParamAttribute(string queryParameterName)
    {
        QueryParameterName = queryParameterName ?? throw new ArgumentNullException(nameof(queryParameterName));
    }

    public string QueryParameterName { get; }

    public void Apply(ActionModel action)
    {
        foreach (var selector in action.Selectors)
        {
            var endpointMetadata = selector.EndpointMetadata;

            var redactedInfo = endpointMetadata.OfType<RedactedUrlParameters>().SingleOrDefault();
            if (redactedInfo is null)
            {
                redactedInfo = new();
                endpointMetadata.Add(redactedInfo);
            }

            redactedInfo.QueryParameters.Add(QueryParameterName);
        }
    }
}

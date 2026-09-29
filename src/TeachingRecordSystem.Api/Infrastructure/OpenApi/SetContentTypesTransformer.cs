using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

internal class SetContentTypesTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        // Remove invalid Content-Types for a given request/response

        if (operation.RequestBody is OpenApiRequestBody { Content: not null } requestBody)
        {
            foreach (var contentType in requestBody.Content.Keys.ToArray())
            {
                if (contentType is "text/json" or "application/*+json")
                {
                    requestBody.Content.Remove(contentType);
                }
            }
        }

        var problemDetailsStatusCodes = context.Description.SupportedResponseTypes
            .Where(r => r.Type is not null && r.Type.IsAssignableTo(typeof(ProblemDetails)))
            .Select(r => r.StatusCode.ToString())
            .ToHashSet();

        foreach (var (statusCode, response) in operation.Responses ?? [])
        {
            if (response.Content is not null)
            {
                foreach (var contentType in response.Content.Keys.ToArray())
                {
                    if (contentType == "text/json")
                    {
                        response.Content.Remove(contentType);
                    }
                    else if (contentType == "application/json" && problemDetailsStatusCodes.Contains(statusCode))
                    {
                        response.Content.Add("application/problem+json", response.Content[contentType]);
                        response.Content.Remove(contentType);
                    }
                }
            }
        }

        return Task.CompletedTask;
    }
}

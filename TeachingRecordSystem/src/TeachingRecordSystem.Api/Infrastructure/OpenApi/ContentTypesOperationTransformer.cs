using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public class ContentTypesOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        // Remove invalid Content-Types for a given request/response

        if (operation.RequestBody is OpenApiRequestBody requestBody)
        {
            foreach (var contentType in requestBody.Content.Keys.ToArray())
            {
                if (contentType is "text/json" or "application/*+json")
                {
                    requestBody.Content.Remove(contentType);
                }
            }
        }

        foreach (var (key, response) in operation.Responses)
        {
            foreach (var contentType in response.Content.Keys.ToArray())
            {
                if (contentType == "text/json")
                {
                    response.Content.Remove(contentType);
                }
                else if (contentType == "application/json" && response.Content[contentType].Schema.Reference?.ReferenceV3 == "#/components/schemas/ProblemDetails")
                {
                    response.Content.Add("application/problem+json", response.Content[contentType]);
                    response.Content.Remove(contentType);
                }
            }
        }

        return Task.CompletedTask;
    }
}

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

internal class RemoveOperationTagsTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        operation.Tags?.Clear();
        return Task.CompletedTask;
    }
}

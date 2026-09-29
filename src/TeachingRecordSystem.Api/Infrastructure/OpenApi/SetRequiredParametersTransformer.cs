using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

// Microsoft.AspNetCore.OpenApi ignores [Required] on nullable parameters (e.g. DateTime?)
internal class SetRequiredParametersTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var requiredParameterNames = context.Description.ParameterDescriptions
            .Where(p => p.ModelMetadata is DefaultModelMetadata { Attributes.Attributes: var attributes } &&
                attributes.OfType<RequiredAttribute>().Any())
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var parameter in (operation.Parameters ?? []).OfType<OpenApiParameter>())
        {
            if (parameter.Name is not null && requiredParameterNames.Contains(parameter.Name))
            {
                parameter.Required = true;
            }
        }

        return Task.CompletedTask;
    }
}

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

internal class AddMinorVersionHeaderTransformer(string minorVersion) : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var headerValueSchemaName = "VersionHeader";

        document.Components ??= new OpenApiComponents();
        document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();
        document.Components.Schemas.Add(headerValueSchemaName, new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            Const = minorVersion
        });

        foreach (var (_, path) in document.Paths)
        {
            foreach (var (_, op) in path.Operations!)
            {
                op.Parameters ??= new List<IOpenApiParameter>();
                op.Parameters.Add(new OpenApiParameter()
                {
                    Name = VersionRegistry.MinorVersionHeaderName,
                    Required = true,
                    In = ParameterLocation.Header,
                    Schema = new OpenApiSchema { Id = headerValueSchemaName }
                });
            }
        }

        return Task.CompletedTask;
    }
}

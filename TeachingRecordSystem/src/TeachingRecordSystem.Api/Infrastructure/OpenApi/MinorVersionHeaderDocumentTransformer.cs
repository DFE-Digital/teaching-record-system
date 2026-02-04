using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public class MinorVersionHeaderDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument swaggerDoc, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var documentVersion = swaggerDoc.Info.Version;
        var isMinorVersion = documentVersion.StartsWith("v3_", StringComparison.Ordinal);

        if (!isMinorVersion)
        {
            return Task.CompletedTask;
        }

        var minorVersion = documentVersion["v3_".Length..];

        var headerValueSchemaName = "version-header-value";

        swaggerDoc.Components.Schemas.Add(
            headerValueSchemaName,
            new OpenApiSchema
            {
                Type = "string",
                Enum = [new OpenApiString(minorVersion)] // TODO Use const when OpenAPI supports openapi 3.1
            });

        foreach (var (_, path) in swaggerDoc.Paths)
        {
            foreach (var (_, operation) in path.Operations)
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = VersionRegistry.MinorVersionHeaderName,
                    Required = true,
                    In = ParameterLocation.Header,
                    Schema = new OpenApiSchema
                    {
                        Reference = new OpenApiReference { Id = headerValueSchemaName, Type = ReferenceType.Schema }
                    }
                });
            }
        }

        return Task.CompletedTask;
    }
}

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public class MinorVersionHeaderDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var documentVersion = swaggerDoc.Info.Version;
        var isMinorVersion = documentVersion.StartsWith("v3_", StringComparison.Ordinal);

        if (!isMinorVersion)
        {
            return;
        }

        var minorVersion = documentVersion["v3_".Length..];

        var headerValueSchemaName = "version-header-value";

        swaggerDoc.Components.Schemas.Add(
            headerValueSchemaName,
            new OpenApiSchema
            {
                Type = "string",
                Enum = [new OpenApiString(minorVersion)] // TODO Use const when Swashbuckle supports openapi 3.1
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
    }
}

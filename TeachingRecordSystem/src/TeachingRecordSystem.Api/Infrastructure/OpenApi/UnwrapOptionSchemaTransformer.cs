using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Optional;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public class UnwrapOptionSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Option<>))
        {
            var innerType = type.GetGenericArguments()[0];
            var innerSchema = context.SchemaService.GetOrCreateSchema(innerType, context);
            
            // Copy properties from inner schema to this schema
            schema.Type = innerSchema.Type;
            schema.Format = innerSchema.Format;
            schema.Properties = innerSchema.Properties;
            schema.Items = innerSchema.Items;
            schema.Reference = innerSchema.Reference;
            schema.AllOf = innerSchema.AllOf;
            schema.AnyOf = innerSchema.AnyOf;
            schema.OneOf = innerSchema.OneOf;
        }

        return Task.CompletedTask;
    }
}

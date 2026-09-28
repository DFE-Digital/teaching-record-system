using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using OneOf;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

// OneOf<T0, T1> is serialized as whichever of T0 or T1 it holds. anyOf rather than oneOf since a value can be valid for
// both (e.g. where T1 has a subset of T0's properties).
internal class OneOfSchemaTransformer : IOpenApiSchemaTransformer
{
    public static bool IsOneOfType(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(OneOf<,>);

    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (!IsOneOfType(context.JsonTypeInfo.Type))
        {
            return;
        }

        schema.AnyOf = [];

        foreach (var type in context.JsonTypeInfo.Type.GetGenericArguments())
        {
            schema.AnyOf.Add(await context.GetOrCreateSchemaAsync(type, cancellationToken: cancellationToken));
        }
    }
}

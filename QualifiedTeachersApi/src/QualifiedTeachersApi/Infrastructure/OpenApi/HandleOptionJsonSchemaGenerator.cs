using Namotion.Reflection;
using NJsonSchema;
using NJsonSchema.Generation;
using NSwag.Generation;
using Optional;

namespace QualifiedTeachersApi.Infrastructure.OpenApi;

public class HandleOptionJsonSchemaGenerator : OpenApiSchemaGenerator
{
    public HandleOptionJsonSchemaGenerator(OpenApiDocumentGeneratorSettings settings)
        : base(settings)
    {
    }

    public override TSchemaType GenerateWithReferenceAndNullability<TSchemaType>(
        ContextualType contextualType,
        bool isNullable,
        JsonSchemaResolver schemaResolver,
        Action<TSchemaType, JsonSchema>? transformation = null)
    {
        if (contextualType.Type.IsGenericType && contextualType.Type.GetGenericTypeDefinition() == typeof(Option<>))
        {
            contextualType = contextualType.Type.GetGenericArguments()[0].ToContextualType();
        }

        var schema = base.GenerateWithReferenceAndNullability(contextualType, isNullable, schemaResolver, transformation);

        return schema;
    }
}

using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Optional;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

// An Option<T> property can be omitted entirely, so document it as a non-required property with T's schema
// (nullable when T is).
internal class UnwrapOptionPropertiesTransformer : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.JsonTypeInfo.Kind is not JsonTypeInfoKind.Object || schema.Properties is null)
        {
            return;
        }

        foreach (var property in context.JsonTypeInfo.Properties)
        {
            if (!property.PropertyType.IsGenericType ||
                property.PropertyType.GetGenericTypeDefinition() != typeof(Option<>) ||
                !schema.Properties.ContainsKey(property.Name))
            {
                continue;
            }

            var valueType = property.PropertyType.GetGenericArguments()[0];
            IOpenApiSchema valueSchema = await context.GetOrCreateSchemaAsync(valueType, cancellationToken: cancellationToken);

            if (IsNullable(property, valueType))
            {
                valueSchema = MakeNullable(valueSchema);
            }

            schema.Properties[property.Name] = valueSchema;
            schema.Required?.Remove(property.Name);
        }
    }

    private static bool IsNullable(JsonPropertyInfo property, Type valueType)
    {
        if (Nullable.GetUnderlyingType(valueType) is not null)
        {
            return true;
        }

        if (valueType.IsValueType)
        {
            return false;
        }

        // Reference type nullability is only available from the annotations on the declaring member
        var nullabilityInfo = property.AttributeProvider switch
        {
            PropertyInfo propertyInfo => new NullabilityInfoContext().Create(propertyInfo),
            FieldInfo fieldInfo => new NullabilityInfoContext().Create(fieldInfo),
            _ => null
        };

        return nullabilityInfo?.GenericTypeArguments[0].ReadState is NullabilityState.Nullable;
    }

    private static IOpenApiSchema MakeNullable(IOpenApiSchema schema)
    {
        // Objects and enums are replaced with references to components after transformers have run, which would lose an
        // added null type (and an enum would also need null adding to its values)
        if (schema is OpenApiSchema { Type: { } type } inlineSchema &&
            !type.HasFlag(JsonSchemaType.Object) &&
            inlineSchema.Enum is not { Count: > 0 })
        {
            var nullableSchema = (OpenApiSchema)inlineSchema.CreateShallowCopy();
            nullableSchema.Type = type | JsonSchemaType.Null;
            return nullableSchema;
        }

        return new OpenApiSchema
        {
            OneOf = [new OpenApiSchema { Type = JsonSchemaType.Null }, schema]
        };
    }
}

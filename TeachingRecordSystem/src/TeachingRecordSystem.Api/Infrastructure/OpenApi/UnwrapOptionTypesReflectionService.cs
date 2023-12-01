using Namotion.Reflection;
using NJsonSchema.Generation;
using Optional;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public class UnwrapOptionTypesReflectionService : SystemTextJsonReflectionService
{
    protected override JsonTypeDescription GetDescription(
        ContextualType contextualType,
        SystemTextJsonSchemaGeneratorSettings settings,
        Type originalType,
        bool isNullable,
        ReferenceTypeNullHandling defaultReferenceTypeNullHandling)
    {
        if (contextualType.Type.IsGenericType && contextualType.Type.GetGenericTypeDefinition() == typeof(Option<>))
        {
            originalType = contextualType.Type.GetGenericArguments()[0];
            contextualType = originalType.ToContextualType();
        }

        return base.GetDescription(contextualType, settings, originalType, isNullable, defaultReferenceTypeNullHandling);
    }
}

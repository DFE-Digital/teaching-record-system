using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public class RemoveExcludedEnumOptionsSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        if (!type.IsEnum)
        {
            return Task.CompletedTask;
        }

        foreach (var enumValue in schema.Enum.Cast<OpenApiString>().ToArray())
        {
            var field = type.GetField(enumValue.Value)!;

            if (field.GetCustomAttribute<ExcludeFromSchemaAttribute>() is not null)
            {
                schema.Enum.Remove(enumValue);
            }
        }

        return Task.CompletedTask;
    }
}

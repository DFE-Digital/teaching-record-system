using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public class RemoveEnumValuesForFlagsEnumSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        if (!type.IsEnum)
        {
            return Task.CompletedTask;
        }

        if (type.GetCustomAttribute<FlagsAttribute>() is null)
        {
            return Task.CompletedTask;
        }

        schema.Enum.Clear();

        return Task.CompletedTask;
    }
}

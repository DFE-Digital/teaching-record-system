using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public class RemoveExcludedEnumOptionsSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;

        if (!type.IsEnum)
        {
            return;
        }

        foreach (OpenApiString openApiString in schema.Enum.Cast<OpenApiString>().ToArray())
        {
            var field = type.GetField(openApiString.Value)!;

            if (field.GetCustomAttribute<ExcludeFromSchemaAttribute>() is not null)
            {
                schema.Enum.Remove(openApiString);
            }
        }
    }
}

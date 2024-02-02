using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public class RemoveEnumValuesForFlagsEnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;

        if (!type.IsEnum)
        {
            return;
        }

        if (type.GetCustomAttribute<FlagsAttribute>() is null)
        {
            return;
        }

        schema.Enum.Clear();
    }
}

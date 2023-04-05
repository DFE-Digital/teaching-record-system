#nullable disable
using System;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace QualifiedTeachersApi.Swagger;

public class RemoveNonFlagEnumValuesSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum)
        {
            throw new InvalidOperationException("Filter is only valid for enum types.");
        }

        schema.Enum = Enum.GetNames(context.Type).Zip(Enum.GetValues(context.Type).Cast<int>(), (name, value) => (Name: name, Value: value))
            .Where(t => (t.Value & (t.Value - 1)) == 0 && t.Value != 0)
            .Select(t => new OpenApiString(t.Name))
            .Cast<IOpenApiAny>()
            .ToList();
    }
}

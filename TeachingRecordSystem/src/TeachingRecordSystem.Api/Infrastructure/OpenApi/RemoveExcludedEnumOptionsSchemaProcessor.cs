using System.Reflection;
using NJsonSchema.Generation;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public class RemoveExcludedEnumOptionsSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        var type = context.ContextualType.Type;

        if (!type.IsEnum)
        {
            return;
        }

        foreach (var memberName in Enum.GetNames(type))
        {
            var member = type.GetField(memberName)!;
            if (member.GetCustomAttribute<ExcludeFromSchemaAttribute>() is not null)
            {
                context.Schema.Enumeration.Remove(memberName);
                context.Schema.EnumerationNames.Remove(memberName);
            }
        }
    }
}

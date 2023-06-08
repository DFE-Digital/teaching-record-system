using System.Reflection;
using NJsonSchema.Generation;

namespace QualifiedTeachersApi.Infrastructure.OpenApi;

public class RemoveCompositeValuesFromFlagsEnumSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        // For Flags enums we may have composite values defined as well as a '0' value;
        // we don't want these to appear in the Swagger doc.

        var type = context.ContextualType.Type;

        if (!type.IsEnum)
        {
            return;
        }

        if (type.GetCustomAttribute<FlagsAttribute>() is null)
        {
            return;
        }

        context.Schema.Enumeration.Clear();
        context.Schema.EnumerationNames.Clear();

        var values = Enum.GetNames(type).Zip(Enum.GetValues(type).Cast<int>(), (name, value) => (Name: name, Value: value))
            .Where(t => (t.Value & t.Value - 1) == 0 && t.Value != 0);

        foreach (var v in values)
        {
            context.Schema.Enumeration.Add(v.Name);
            context.Schema.EnumerationNames.Add(v.Name);
        }
    }
}

using NJsonSchema;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public class GeneratedCrmTypeNameGenerator : ITypeNameGenerator
{
    private readonly ITypeNameGenerator _innerGenerator;

    public GeneratedCrmTypeNameGenerator(ITypeNameGenerator innerGenerator)
    {
        _innerGenerator = innerGenerator;
    }

    public string Generate(JsonSchema schema, string? typeNameHint, IEnumerable<string> reservedTypeNames)
    {
        var name = _innerGenerator.Generate(schema, typeNameHint, reservedTypeNames);

        // Generated CRM models for custom entities have a weird type name; fix that up here
        // e.g. for the 'dfeta_inductionState' type use 'InductionState' in the API spec
        if (name.StartsWith("dfeta_", StringComparison.OrdinalIgnoreCase))
        {
            var prefixTrimmedTypeName = name["dfeta_".Length..];
            return prefixTrimmedTypeName[0..1].ToUpper() + prefixTrimmedTypeName[1..];
        }

        return name;
    }
}

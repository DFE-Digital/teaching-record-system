using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public class UnionSerializerDataContractResolver(ISerializerDataContractResolver inner) : ISerializerDataContractResolver
{
    public DataContract GetDataContractForType(Type type)
    {
        if (type.IsDefined(typeof(UnionAttribute), inherit: false))
        {
            // By convention, use the schema of the first case type.
            // Union cases don't depend on serializer options so the default options are fine here.

            var typeInfo = JsonSerializerOptions.Default.GetTypeInfo(type);
            if (typeInfo.Kind is JsonTypeInfoKind.Union)
            {
                return inner.GetDataContractForType(typeInfo.UnionCases[0].CaseType);
            }
        }

        return inner.GetDataContractForType(type);
    }
}

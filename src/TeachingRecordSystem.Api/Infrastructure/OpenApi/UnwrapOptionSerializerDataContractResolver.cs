using Optional;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public class UnwrapOptionSerializerDataContractResolver(ISerializerDataContractResolver inner) : ISerializerDataContractResolver
{
    public DataContract GetDataContractForType(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Option<>))
        {
            return inner.GetDataContractForType(type.GetGenericArguments()[0]);
        }

        return inner.GetDataContractForType(type);
    }
}

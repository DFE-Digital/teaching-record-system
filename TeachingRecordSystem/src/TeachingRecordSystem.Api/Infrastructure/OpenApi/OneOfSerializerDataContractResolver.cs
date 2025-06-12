using OneOf;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public class OneOfSerializerDataContractResolver(ISerializerDataContractResolver inner) : ISerializerDataContractResolver
{
    public DataContract GetDataContractForType(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(OneOf<,>))
        {
            // By convention, use the schema of the T0 in OneOf<T0, T1>.

            return inner.GetDataContractForType(type.GetGenericArguments()[0]);
        }

        return inner.GetDataContractForType(type);
    }
}

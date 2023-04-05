#nullable disable
using Swashbuckle.AspNetCore.Annotations;

namespace QualifiedTeachersApi.V2.Responses;

public class IttProviderInfo
{
    [SwaggerSchema(Nullable = false)]
    public string Ukprn { get; set; }

    [SwaggerSchema(Nullable = false)]
    public string ProviderName { get; set; }
}

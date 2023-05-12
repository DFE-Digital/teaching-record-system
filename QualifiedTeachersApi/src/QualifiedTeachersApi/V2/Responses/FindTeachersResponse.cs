#nullable disable
using Swashbuckle.AspNetCore.Annotations;

namespace QualifiedTeachersApi.V2.Responses;

public class FindTeachersResponse
{
    [SwaggerSchema(Nullable = false)]
    public IEnumerable<FindTeacherResult> Results { get; set; }
}

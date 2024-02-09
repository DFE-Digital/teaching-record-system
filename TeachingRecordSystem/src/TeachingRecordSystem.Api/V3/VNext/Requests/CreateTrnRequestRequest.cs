using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Api.V3.VNext.ApiModels;

namespace TeachingRecordSystem.Api.V3.VNext.Requests;

public record CreateTrnRequestRequest
{
    [SwaggerSchema(description:
        "A unique ID that represents this request. " +
        "If a request has already been created with this ID then that existing record's result is returned.")]
    public required string RequestId { get; init; }
    public required TrnRequestPerson Person { get; init; }
}

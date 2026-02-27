using TeachingRecordSystem.Core.ApiSchema.V3.V20240307.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240307.Requests;

public record CreateTrnRequestRequest
{
    [SwaggerSchema(description:
        "A unique ID that represents this request. " +
        "If a request has already been created with this ID then that existing record's result is returned.")]
    public required string RequestId { get; init; }
    public required TrnRequestPerson Person { get; init; }
}

using Swashbuckle.AspNetCore.Annotations;

namespace TeachingRecordSystem.Api.V3.V20240606.Requests;

public record CreateTrnRequestRequest
{
    [SwaggerSchema(description:
        "A unique ID that represents this request. " +
        "If a request has already been created with this ID then that existing record's result is returned.")]
    public required string RequestId { get; init; }
    public required CreateTrnRequestRequestPerson Person { get; init; }
}

public record CreateTrnRequestRequestPerson
{
    public required string FirstName { get; init; }
    public string? MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public IReadOnlyCollection<string>? EmailAddresses { get; init; }
    public string? NationalInsuranceNumber { get; init; }
}

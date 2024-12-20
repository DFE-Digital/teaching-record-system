using Swashbuckle.AspNetCore.Annotations;
using TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos;

namespace TeachingRecordSystem.Api.V3.VNext.Requests;

public record CreateTrnRequestRequest
{
    [SwaggerSchema(description:
        "A unique ID that represents this request. " +
        "If a request has already been created with this ID then that existing record's result is returned.")]
    public required string RequestId { get; init; }
    public required CreateTrnRequestRequestPerson Person { get; init; }
    public bool IdentityVerified { get; init; }
    public string? OneLoginUserSubject { get; init; }
}

public record CreateTrnRequestRequestPerson
{
    public required string FirstName { get; init; }
    public string? MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public IReadOnlyCollection<string>? EmailAddresses { get; init; }
    public string? NationalInsuranceNumber { get; init; }
    public CreateTrnRequestAddress? Address { get; init; }
    public Gender? Gender { get; init; }
}

public class CreateTrnRequestAddress
{
    public string? AddressLine1 { get; init; }
    public string? AddressLine2 { get; init; }
    public string? AddressLine3 { get; init; }
    public string? City { get; init; }
    public string? Postcode { get; init; }
    public string? Country { get; init; }
}


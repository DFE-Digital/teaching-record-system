using Gender = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos.Gender;

namespace TeachingRecordSystem.Api.V3.V20250425.Requests;

public record SetPiiRequest
{
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public string? EmailAddress { get; init; }
    public string? NationalInsuranceNumber { get; init; }
    public Gender? Gender { get; init; }
}

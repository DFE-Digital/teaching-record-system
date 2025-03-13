using TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos;

namespace TeachingRecordSystem.Api.V3.VNext.Requests;

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

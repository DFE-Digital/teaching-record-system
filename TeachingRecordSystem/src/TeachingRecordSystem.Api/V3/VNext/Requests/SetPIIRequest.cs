

using TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos;

namespace TeachingRecordSystem.Api.V3.VNext.Requests;

public record SetPIIRequest
{
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required IReadOnlyCollection<string> EmailAddresses { get; init; }
    public required string NationalInsuranceNumber { get; init; }
    public required Gender? Gender { get; init; }
}

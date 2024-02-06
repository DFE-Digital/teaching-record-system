namespace TeachingRecordSystem.Api.V3.Core.SharedModels;

public record TrnRequestPerson
{
    public required string FirstName { get; init; }
    public required string? MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string? Email { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
}

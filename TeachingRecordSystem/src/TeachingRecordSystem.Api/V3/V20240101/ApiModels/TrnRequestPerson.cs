namespace TeachingRecordSystem.Api.V3.V20240101.ApiModels;

public record TrnRequestPerson
{
    public required string FirstName { get; init; }
    public string? MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public string? Email { get; init; }
    public string? NationalInsuranceNumber { get; init; }
}
